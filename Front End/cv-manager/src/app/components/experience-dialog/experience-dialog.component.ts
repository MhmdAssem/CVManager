import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { CustomValidators } from '../../Validators/CustomValidators';

@Component({
  selector: 'app-experience-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './experience-dialog.component.html',
  styleUrls: ['./experience-dialog.component.scss']
})
export class ExperienceDialogComponent {
  experienceForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<ExperienceDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.experienceForm = this.fb.group({
      companyName: [data?.companyName || '', [Validators.required, CustomValidators.noWhitespace]],
      city: [data?.city || '', [Validators.required, CustomValidators.noWhitespace]],
      companyField: [data?.companyField || '', [Validators.required, CustomValidators.noWhitespace]]
    });
  }

  onSubmit(): void {
    if (this.experienceForm.valid) {
      this.dialogRef.close(this.experienceForm.value);
    }else {
      this.experienceForm.markAllAsTouched();
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  
}
