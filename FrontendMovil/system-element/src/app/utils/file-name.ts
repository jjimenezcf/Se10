export class FileName {
  static generateNameFromDate(d: Date): string {
    return (
      [d.getFullYear(), d.getMonth() + 1, d.getDate()].join('-') +
      [d.getHours(), d.getMinutes(), d.getSeconds()].join('')
    );
  }
}
