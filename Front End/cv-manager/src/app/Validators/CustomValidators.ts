import { AbstractControl, ValidationErrors } from '@angular/forms';

export class CustomValidators {
  static noWhitespace(control: AbstractControl): ValidationErrors | null {
    if (control.value != null && typeof control.value === 'string' && control.value.trim().length === 0) {
      return { noWhitespace: true };
    }
    return null;
  }
}
