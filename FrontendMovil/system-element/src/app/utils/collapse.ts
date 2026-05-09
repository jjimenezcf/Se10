export class Collapsible<T extends string | number | symbol> {
  private _items: { [key in T]?: boolean } = {};

  get items() {
    return this._items;
  }

  toggle(key: T) {
    this._items[key] = !this._items[key];
  }

  clear() {
    this._items = {};
  }

  remove(key: T) {
    delete this._items[key];
  }

  get(key: T) {
    return this._items[key] ?? null;
  }

  setAll(value: boolean) {
    Object.keys(this._items).forEach((k) => (this._items[k] = value));
  }
}
