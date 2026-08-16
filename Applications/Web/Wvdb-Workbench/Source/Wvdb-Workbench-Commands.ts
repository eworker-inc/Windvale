import type { Commandˉtab } from '../../../../Libraries/Web/Components/Command-Surface/Command-Surface';

export function Buildˉcommandˉtabs(Text: (Identifier: string) => string): readonly Commandˉtab[] {
  const Command = (Identifier: string, Label: string, Glyph: string, Enabled: boolean) => ({
    Identifier,
    Label: Text(Label),
    Glyph,
    Enabled
  });
  return [
    {
      Identifier: 'home', Label: Text('ribbon.home'), Groups: [
        {
          Label: Text('group.query'), Commands: [
            Command('query.new', 'command.new_query', '+', true),
            Command('query.validate', 'command.validate', '✓', true),
            Command('query.execute', 'command.execute', '▶', false)
          ]
        },
        {
          Label: Text('group.view'), Commands: [
            Command('view.explorer', 'command.explorer', '☷', true),
            Command('view.console', 'command.console', '▤', true),
            Command('view.assistant', 'command.assistant', 'AI', true)
          ]
        },
        {
          Label: Text('group.connection'), Commands: [
            Command('server.connect', 'command.connect', '↗', false),
            Command('server.disconnect', 'command.disconnect', '×', false)
          ]
        }
      ]
    },
    {
      Identifier: 'query', Label: Text('ribbon.query'), Groups: [
        {
          Label: Text('group.query'), Commands: [
            Command('query.new', 'command.new_query', '+', true),
            Command('query.validate', 'command.validate', '✓', true),
            Command('query.execute', 'command.execute', '▶', false)
          ]
        },
        { Label: Text('group.results'), Commands: [Command('query.export', 'command.export', '⇩', false)] },
        {
          Label: Text('group.assistant'), Commands: [
            Command('ai.focus', 'command.ask', 'AI', true),
            Command('ai.explain', 'command.explain', '◇', false)
          ]
        }
      ]
    },
    {
      Identifier: 'data', Label: Text('ribbon.data'), Groups: [
        {
          Label: Text('group.records'), Commands: [
            Command('data.browse', 'command.browse', '▦', true),
            Command('data.new', 'command.new_record', '+', false),
            Command('data.save', 'command.save', '◉', false)
          ]
        },
        { Label: Text('group.results'), Commands: [Command('data.export', 'command.export', '⇩', false)] }
      ]
    },
    {
      Identifier: 'schema', Label: Text('ribbon.schema'), Groups: [
        {
          Label: Text('group.schema'), Commands: [
            Command('schema.collection', 'command.collection', '◫', false),
            Command('schema.index', 'command.index', '⌁', false)
          ]
        }
      ]
    },
    {
      Identifier: 'operations', Label: Text('ribbon.operations'), Groups: [
        {
          Label: Text('group.operations'), Commands: [
            Command('operations.backup', 'command.backup', '⇩', false),
            Command('operations.restore', 'command.restore', '↥', false)
          ]
        }
      ]
    },
    {
      Identifier: 'ai', Label: Text('ribbon.ai'), Groups: [
        {
          Label: Text('group.assistant'), Commands: [
            Command('ai.focus', 'command.ask', 'AI', true),
            Command('ai.explain', 'command.explain', '◇', false)
          ]
        }
      ]
    }
  ];
}
