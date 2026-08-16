export interface Assistantˉmessage {
  readonly Identifier: string;
  readonly Role: 'assistant' | 'user';
  readonly Text: string;
  readonly Meta?: string;
}

export interface Assistantˉpanelˉcopy {
  readonly Title: string;
  readonly Preview: string;
  readonly Emptyˉhint: string;
  readonly Placeholder: string;
  readonly Send: string;
  readonly Collapse: string;
}

export class Assistantˉpanel {
  readonly #Host: HTMLElement;
  readonly #Onˉsend: (Text: string) => void;
  readonly #Onˉcollapse: () => void;

  constructor(Host: HTMLElement, Onˉsend: (Text: string) => void, Onˉcollapse: () => void) {
    this.#Host = Host;
    this.#Onˉsend = Onˉsend;
    this.#Onˉcollapse = Onˉcollapse;
  }

  Render(Copy: Assistantˉpanelˉcopy, Messages: readonly Assistantˉmessage[]): void {
    const Panel = document.createElement('section');
    Panel.className = 'wv-assistant-panel';
    const Header = document.createElement('header');
    const Heading = document.createElement('div');
    const Title = document.createElement('strong');
    Title.textContent = Copy.Title;
    const Preview = document.createElement('span');
    Preview.textContent = Copy.Preview;
    Heading.append(Title, Preview);
    const Collapse = document.createElement('button');
    Collapse.type = 'button';
    Collapse.className = 'wv-panel-collapse';
    Collapse.textContent = '›';
    Collapse.title = Copy.Collapse;
    Collapse.setAttribute('aria-label', Copy.Collapse);
    Collapse.addEventListener('click', this.#Onˉcollapse);
    Header.append(Heading, Collapse);

    const Context = document.createElement('div');
    Context.className = 'wv-assistant-context';
    Context.textContent = Copy.Emptyˉhint;
    const Feed = document.createElement('div');
    Feed.className = 'wv-assistant-feed';
    Feed.setAttribute('role', 'log');
    Feed.setAttribute('aria-live', 'polite');
    for (const Message of Messages) {
      const Item = document.createElement('article');
      Item.className = `wv-assistant-message wv-assistant-message--${Message.Role}`;
      const Role = document.createElement('span');
      Role.className = 'wv-assistant-avatar';
      Role.textContent = Message.Role === 'assistant' ? 'WV' : 'You';
      const Body = document.createElement('div');
      const Text = document.createElement('p');
      Text.textContent = Message.Text;
      Body.append(Text);
      if (Message.Meta !== undefined) {
        const Meta = document.createElement('small');
        Meta.textContent = Message.Meta;
        Body.append(Meta);
      }
      Item.append(Role, Body);
      Feed.append(Item);
    }

    const Form = document.createElement('form');
    Form.className = 'wv-assistant-composer';
    const Input = document.createElement('textarea');
    Input.rows = 2;
    Input.maxLength = 2000;
    Input.placeholder = Copy.Placeholder;
    const Send = document.createElement('button');
    Send.type = 'submit';
    Send.textContent = Copy.Send;
    Form.append(Input, Send);
    Form.addEventListener('submit', (Event) => {
      Event.preventDefault();
      const Text = Input.value.trim();
      if (Text.length > 0) {
        this.#Onˉsend(Text);
      }
    }, { once: true });
    Panel.append(Header, Context, Feed, Form);
    this.#Host.replaceChildren(Panel);
    Feed.scrollTop = Feed.scrollHeight;
  }
}
