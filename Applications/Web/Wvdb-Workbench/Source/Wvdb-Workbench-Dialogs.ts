import type { Connectionˉprofile, Connectionˉprofileˉdraft, Connectionˉprofileˉresult } from './Connection-Profile';
import type { Wvdbˉworkbenchˉstate } from './Wvdb-Workbench-State';

type Textˉreader = (Identifier: string) => string;
export interface Wvdbˉworkbenchˉdialogˉactions {
  readonly Saveˉprofile: (Draft: Connectionˉprofileˉdraft) => Connectionˉprofileˉresult;
  readonly Deleteˉprofile: (Identifier: string) => boolean;
  readonly Applyˉsettings: (Theme: Wvdbˉworkbenchˉstate['Theme'], Locale: Wvdbˉworkbenchˉstate['Locale']) => void;
  readonly Resetˉlayout: () => void;
}

export class Wvdbˉworkbenchˉdialogs {
  readonly #Host: HTMLElement; readonly #Actions: Wvdbˉworkbenchˉdialogˉactions;
  #Active?: HTMLDialogElement;
  constructor(Host: HTMLElement, Actions: Wvdbˉworkbenchˉdialogˉactions) { this.#Host = Host; this.#Actions = Actions; }

  Openˉconnection(Text: Textˉreader, Profile?: Connectionˉprofile): void {
    this.#Closeˉactive();
    const Dialog = document.createElement('dialog'); Dialog.className = 'wv-native-dialog';
    const Form = document.createElement('form'); Form.method = 'dialog';
    const Header = this.#Header(Text(Profile === undefined ? 'dialog.connection_title' : 'dialog.connection_edit'), Text('dialog.connection_copy'));
    const Body = document.createElement('div'); Body.className = 'wv-dialog-body';
    const Name = this.#Field(Body, Text('dialog.name'), 'text', 64, Profile?.Displayˉname ?? '');
    const Endpoint = this.#Field(Body, Text('dialog.endpoint'), 'url', 512, Profile?.Endpoint ?? 'http://127.0.0.1:7412');
    const Database = this.#Field(Body, Text('dialog.database'), 'text', 64, Profile?.Defaultˉdatabase ?? 'Development');
    const Note = document.createElement('p'); Note.className = 'wv-dialog-note'; Note.textContent = Text('dialog.profile_note');
    const Error = document.createElement('p'); Error.className = 'wv-dialog-error'; Error.setAttribute('role', 'alert'); Body.append(Note, Error);
    const Footer = document.createElement('footer');
    if (Profile !== undefined) {
      const Delete = this.#Button(Text('dialog.delete'), 'button'); Delete.className = 'wv-dialog-danger';
      Delete.addEventListener('click', () => { if (this.#Actions.Deleteˉprofile(Profile.Identifier)) Dialog.close(); else Error.textContent = Text('dialog.error_storage'); }); Footer.append(Delete);
    }
    const Cancel = this.#Button(Text('dialog.cancel'), 'button'); Cancel.addEventListener('click', () => Dialog.close());
    const Save = this.#Button(Text('dialog.save_profile'), 'submit'); Save.className = 'wv-dialog-primary'; Footer.append(Cancel, Save);
    Form.addEventListener('submit', (Event) => {
      Event.preventDefault();
      const Result = this.#Actions.Saveˉprofile({ Identifier: Profile?.Identifier, Displayˉname: Name.value, Endpoint: Endpoint.value, Defaultˉdatabase: Database.value });
      if (Result.Ok) Dialog.close(); else Error.textContent = Text(`dialog.error_${Result.Error}`);
    });
    Form.append(Header, Body, Footer); Dialog.append(Form); this.#Mount(Dialog); requestAnimationFrame(() => Name.focus());
  }

  Openˉsettings(Text: Textˉreader, State: Wvdbˉworkbenchˉstate): void {
    this.#Closeˉactive();
    const Dialog = document.createElement('dialog'); Dialog.className = 'wv-native-dialog wv-settings-dialog';
    const Form = document.createElement('form'); Form.method = 'dialog';
    const Header = this.#Header(Text('settings.title'), Text('settings.copy'));
    const Body = document.createElement('div'); Body.className = 'wv-dialog-body';
    const Theme = document.createElement('select'); Theme.append(new Option(Text('settings.dark'), 'dark'), new Option(Text('settings.light'), 'light')); Theme.value = State.Theme;
    const Locale = document.createElement('select'); Locale.append(new Option('English', 'en'), new Option('Français', 'fr')); Locale.value = State.Locale;
    this.#Labeledˉcontrol(Body, Text('settings.theme'), Theme); this.#Labeledˉcontrol(Body, Text('settings.language'), Locale);
    const Storage = document.createElement('p'); Storage.className = 'wv-dialog-note'; Storage.textContent = Text('settings.storage'); Body.append(Storage);
    const Reset = this.#Button(Text('settings.reset_layout'), 'button'); Reset.addEventListener('click', () => this.#Actions.Resetˉlayout()); Body.append(Reset);
    const Footer = document.createElement('footer'); const Cancel = this.#Button(Text('dialog.cancel'), 'button'); Cancel.addEventListener('click', () => Dialog.close());
    const Save = this.#Button(Text('settings.apply'), 'submit'); Save.className = 'wv-dialog-primary'; Footer.append(Cancel, Save);
    Form.addEventListener('submit', (Event) => { Event.preventDefault(); this.#Actions.Applyˉsettings(Theme.value as Wvdbˉworkbenchˉstate['Theme'], Locale.value as Wvdbˉworkbenchˉstate['Locale']); Dialog.close(); });
    Form.append(Header, Body, Footer); Dialog.append(Form); this.#Mount(Dialog); requestAnimationFrame(() => Theme.focus());
  }

  #Mount(Dialog: HTMLDialogElement): void {
    this.#Active = Dialog; this.#Host.append(Dialog);
    Dialog.addEventListener('close', () => { if (this.#Active === Dialog) this.#Active = undefined; Dialog.remove(); }, { once: true });
    Dialog.addEventListener('cancel', () => Dialog.close()); Dialog.showModal();
  }
  #Closeˉactive(): void { this.#Active?.close(); }
  #Header(Titleˉtext: string, Copyˉtext: string): HTMLElement {
    const Header = document.createElement('header'); const Title = document.createElement('h2'); Title.textContent = Titleˉtext;
    const Copy = document.createElement('p'); Copy.textContent = Copyˉtext; Header.append(Title, Copy); return Header;
  }
  #Field(Host: HTMLElement, Labelˉtext: string, Type: string, Max: number, Value: string): HTMLInputElement {
    const Input = document.createElement('input'); Input.type = Type; Input.maxLength = Max; Input.value = Value; Input.required = true;
    this.#Labeledˉcontrol(Host, Labelˉtext, Input); return Input;
  }
  #Labeledˉcontrol(Host: HTMLElement, Labelˉtext: string, Control: HTMLElement): void {
    const Label = document.createElement('label'); const Copy = document.createElement('span'); Copy.textContent = Labelˉtext; Label.append(Copy, Control); Host.append(Label);
  }
  #Button(Label: string, Type: 'button' | 'submit'): HTMLButtonElement { const Button = document.createElement('button'); Button.type = Type; Button.textContent = Label; return Button; }
}
