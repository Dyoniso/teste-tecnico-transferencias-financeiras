import {
  Component,
  input,
  output,
} from '@angular/core';
import {
  LucidePencil,
  LucideTrash2,
  LucideUserPlus,
} from '@lucide/angular';

import { Person } from '../../models/api.models';

@Component({
  selector: 'app-person-list',
  imports: [
    LucidePencil,
    LucideTrash2,
    LucideUserPlus,
  ],
  templateUrl: './person-list.html',
})
export class PersonList {
  readonly persons = input.required<Person[]>();

  readonly edit = output<Person>();
  readonly remove = output<Person>();
  readonly createAccount = output<Person>();
}