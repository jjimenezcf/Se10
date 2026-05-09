import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  forwardRef,
} from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faSearch } from '@fortawesome/free-solid-svg-icons';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

export const INPUT_CONTROL_VALUE_ACCESSOR: any = {
  provide: NG_VALUE_ACCESSOR,
  useExisting: forwardRef(() => SearchInputComponent),
  multi: true,
};

@Component({
  selector: 'app-search-input',
  standalone: true,
  imports: [CommonModule, FontAwesomeModule, FormsModule],
  template: `
    <form>
      <div class="relative">
        <div
          class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none"
        >
          <fa-icon
            class="text-gray-400"
            [icon]="searchIcon"
            [fixedWidth]="true"
          ></fa-icon>
        </div>
        <input
          class="bg-white border border-gray-300  text-gray-900 text-sm rounded-sm focus:outline-none focus:border-sky-200 focus:ring-2 focus:ring-sky-300 block w-full p-3.5 !pl-10"
          [(ngModel)]="value"
          (input)="onSearch($event.target['value'])"
          [placeholder]="placeholderText"
          id="searchBox"
          name="search"
        />
      </div>
    </form>
  `,
})
export class SearchInputComponent implements OnInit {
  @Input()
  set value(value: any) {
    this._value = value;
    this.onChange(value);
  }
  get value(): any {
    return this._value;
  }
  @Input() placeholderText: string = 'Buscar...';
  @Input() debounceTime = 300;
  @Output() search = new EventEmitter<string>();
  isDisabled: boolean;

  private _value: any;
  protected searchIcon = faSearch;
  private _searchTerm = new Subject<string>();

  constructor() {}

  onChange = (event) => {};
  onTouched = () => {};

  writeValue(value: any) {
    this.value = value;
  }

  registerOnChange(fn: any) {
    this.onChange = fn;
  }

  registerOnTouched(fn: any) {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean) {
    this.isDisabled = isDisabled;
  }

  ngOnInit(): void {
    this._searchTerm
      .pipe(debounceTime(this.debounceTime), distinctUntilChanged())
      .subscribe((searchedText) => {
        this.search.emit(searchedText);
      });
  }

  onSearch(term: string) {
    this._searchTerm.next(term);
  }
}
