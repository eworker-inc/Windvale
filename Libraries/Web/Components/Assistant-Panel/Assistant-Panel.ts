export interface Assistantˉmessage { readonly Identifier: string; readonly Role: 'assistant' | 'user'; readonly Text: string; readonly Meta?: string; }
export interface Assistantˉpanelˉcopy {
  readonly Title: string; readonly Preview: string; readonly Emptyˉhint: string; readonly Placeholder: string;
  readonly Send: string; readonly Collapse: string; readonly Newˉchat: string; readonly Queryˉcontext: string;
  readonly Schemaˉcontext: string; readonly Sessionˉonly: string; readonly Suggestions: readonly string[];
}
export interface Assistantˉpanelˉoptions {
  readonly Onˉsend: (Text: string) => void; readonly Onˉcollapse: () => void;
  readonly Onˉnewˉchat: () => void; readonly Onˉcontext: (Context: 'query' | 'schema') => void;
}
export class Assistantˉpanel {
  readonly #Host: HTMLElement; readonly #Options: Assistantˉpanelˉoptions;
  constructor(Host: HTMLElement, Options: Assistantˉpanelˉoptions) { this.#Host = Host; this.#Options = Options; }
  Render(Copy: Assistantˉpanelˉcopy, Messages: readonly Assistantˉmessage[], Query: boolean, Schema: boolean): void {
    const Panel = document.createElement('section'); Panel.className = 'wv-assistant-panel';
    const Header = document.createElement('header'); const Heading = document.createElement('div');
    const Title = document.createElement('strong'); Title.textContent = Copy.Title;
    const Preview = document.createElement('span'); Preview.textContent = Copy.Preview; Heading.append(Title, Preview);
    const Actions = document.createElement('div'); Actions.className = 'wv-assistant-header-actions';
    const Newˉchat = document.createElement('button'); Newˉchat.type = 'button'; Newˉchat.textContent = '+';
    Newˉchat.title = Copy.Newˉchat; Newˉchat.setAttribute('aria-label', Copy.Newˉchat); Newˉchat.addEventListener('click', this.#Options.Onˉnewˉchat);
    const Collapse = document.createElement('button'); Collapse.type = 'button'; Collapse.className = 'wv-panel-collapse';
    Collapse.textContent = '›'; Collapse.title = Copy.Collapse; Collapse.setAttribute('aria-label', Copy.Collapse);
    Collapse.addEventListener('click', this.#Options.Onˉcollapse); Actions.append(Newˉchat, Collapse); Header.append(Heading, Actions);
    const Context = document.createElement('div'); Context.className = 'wv-assistant-context';
    const Contextˉcopy = document.createElement('p'); Contextˉcopy.textContent = Copy.Emptyˉhint;
    const Chips = document.createElement('div');
    for (const [Kind, Label, Active] of [['query', Copy.Queryˉcontext, Query], ['schema', Copy.Schemaˉcontext, Schema]] as const) {
      const Chip = document.createElement('button'); Chip.type = 'button'; Chip.textContent = Label;
      Chip.setAttribute('aria-pressed', String(Active)); Chip.addEventListener('click', () => this.#Options.Onˉcontext(Kind)); Chips.append(Chip);
    }
    Context.append(Contextˉcopy, Chips);
    const Feed = document.createElement('div'); Feed.className = 'wv-assistant-feed'; Feed.setAttribute('role', 'log'); Feed.setAttribute('aria-live', 'polite');
    for (const Message of Messages) {
      const Item = document.createElement('article'); Item.className = `wv-assistant-message wv-assistant-message--${Message.Role}`;
      const Role = document.createElement('span'); Role.className = 'wv-assistant-avatar'; Role.textContent = Message.Role === 'assistant' ? 'WV' : 'You';
      const Body = document.createElement('div'); const Messageˉtext = document.createElement('p'); Messageˉtext.textContent = Message.Text; Body.append(Messageˉtext);
      if (Message.Meta !== undefined) { const Meta = document.createElement('small'); Meta.textContent = Message.Meta; Body.append(Meta); }
      Item.append(Role, Body); Feed.append(Item);
    }
    if (Messages.length <= 1) {
      const Suggestions = document.createElement('div'); Suggestions.className = 'wv-assistant-suggestions';
      for (const Suggestion of Copy.Suggestions.slice(0, 3)) {
        const Button = document.createElement('button'); Button.type = 'button'; Button.textContent = Suggestion;
        Button.addEventListener('click', () => this.#Options.Onˉsend(Suggestion)); Suggestions.append(Button);
      }
      Feed.append(Suggestions);
    }
    const Form = document.createElement('form'); Form.className = 'wv-assistant-composer';
    const Input = document.createElement('textarea'); Input.rows = 2; Input.maxLength = 2000; Input.placeholder = Copy.Placeholder;
    const Footer = document.createElement('div'); const Limit = document.createElement('small'); Limit.textContent = `0 / ${Input.maxLength}`;
    const Session = document.createElement('small'); Session.textContent = Copy.Sessionˉonly;
    const Send = document.createElement('button'); Send.type = 'submit'; Send.textContent = Copy.Send; Footer.append(Session, Limit, Send); Form.append(Input, Footer);
    Input.addEventListener('input', () => { Limit.textContent = `${Input.value.length} / ${Input.maxLength}`; });
    Form.addEventListener('submit', (Event) => { Event.preventDefault(); const Text = Input.value.trim(); if (Text.length > 0) this.#Options.Onˉsend(Text); });
    Panel.append(Header, Context, Feed, Form); this.#Host.replaceChildren(Panel); Feed.scrollTop = Feed.scrollHeight;
  }
}
