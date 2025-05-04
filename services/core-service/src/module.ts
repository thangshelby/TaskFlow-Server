export type ClassType<T = any> = new (...args: any[]) => T;

export interface Module {
  rests?: ClassType[];
  services?: ClassType[];
  modules?: Module[];
}
