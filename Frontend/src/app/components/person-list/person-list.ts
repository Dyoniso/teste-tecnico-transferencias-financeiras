import {
  Component,
  input,
  output,
} from '@angular/core';

import { Person } from '../../models/api.models';

@Component({
  selector: 'app-person-list',
  templateUrl: './person-list.html',
})
export class PersonList {
  readonly persons = input.required<Person[]>();

  readonly edit = output<Person>();
  readonly remove = output<Person>();
  readonly createAccount = output<Person>();
}