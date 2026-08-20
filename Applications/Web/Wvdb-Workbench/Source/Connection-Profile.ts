export interface Connectionˉprofile {
  readonly Identifier: string;
  readonly Displayˉname: string;
  readonly Endpoint: string;
  readonly Defaultˉdatabase: string;
}

export interface Connectionˉprofileˉdraft {
  readonly Identifier?: string;
  readonly Displayˉname: string;
  readonly Endpoint: string;
  readonly Defaultˉdatabase: string;
}

export type Connectionˉprofileˉresult =
  | { readonly Ok: true; readonly Profile: Connectionˉprofile }
  | { readonly Ok: false; readonly Error: 'name' | 'endpoint' | 'database' | 'storage' };

export const CONNECTION_PROFILE_LIMIT = 8;
const STORAGE_KEY = 'wvdb-workbench.connections.v1';

function Isˉrecord(Value: unknown): Value is Record<string, unknown> {
  return typeof Value === 'object' && Value !== null && !Array.isArray(Value);
}

export function Validateˉconnectionˉprofile(Draft: Connectionˉprofileˉdraft): Connectionˉprofileˉresult {
  const Displayˉname = Draft.Displayˉname.trim();
  const Defaultˉdatabase = Draft.Defaultˉdatabase.trim();
  if (Displayˉname.length < 1 || Displayˉname.length > 64) return { Ok: false, Error: 'name' };
  if (Defaultˉdatabase.length < 1 || Defaultˉdatabase.length > 64) return { Ok: false, Error: 'database' };
  let Address: URL;
  try { Address = new URL(Draft.Endpoint.trim()); } catch { return { Ok: false, Error: 'endpoint' }; }
  const Loopback = ['localhost', '127.0.0.1', '[::1]'].includes(Address.hostname);
  if ((Address.protocol !== 'https:' && !(Address.protocol === 'http:' && Loopback)) ||
      Address.username.length > 0 || Address.password.length > 0 || Address.hash.length > 0 ||
      Address.href.length > 512) return { Ok: false, Error: 'endpoint' };
  return {
    Ok: true,
    Profile: Object.freeze({
      Identifier: Draft.Identifier ?? crypto.randomUUID(),
      Displayˉname,
      Endpoint: Address.href.replace(/\/$/, ''),
      Defaultˉdatabase
    })
  };
}

export function Readˉconnectionˉprofiles(): readonly Connectionˉprofile[] {
  try {
    const Raw = localStorage.getItem(STORAGE_KEY);
    if (Raw === null || Raw.length > 16_384) return Object.freeze([]);
    const Parsed: unknown = JSON.parse(Raw);
    if (!Array.isArray(Parsed)) return Object.freeze([]);
    const Profiles: Connectionˉprofile[] = [];
    for (const Value of Parsed.slice(0, CONNECTION_PROFILE_LIMIT)) {
      if (!Isˉrecord(Value)) continue;
      const Result = Validateˉconnectionˉprofile({
        Identifier: typeof Value['Identifier'] === 'string' && Value['Identifier'].length <= 64 ? Value['Identifier'] : undefined,
        Displayˉname: typeof Value['Displayˉname'] === 'string' ? Value['Displayˉname'] : '',
        Endpoint: typeof Value['Endpoint'] === 'string' ? Value['Endpoint'] : '',
        Defaultˉdatabase: typeof Value['Defaultˉdatabase'] === 'string' ? Value['Defaultˉdatabase'] : ''
      });
      if (Result.Ok) Profiles.push(Result.Profile);
    }
    return Object.freeze(Profiles);
  } catch { return Object.freeze([]); }
}

export function Writeˉconnectionˉprofiles(Profiles: readonly Connectionˉprofile[]): boolean {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(Profiles.slice(0, CONNECTION_PROFILE_LIMIT)));
    return true;
  } catch { return false; }
}
