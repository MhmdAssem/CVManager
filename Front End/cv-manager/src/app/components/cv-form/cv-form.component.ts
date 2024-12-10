import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ExperienceDialogComponent } from '../experience-dialog/experience-dialog.component';
import { ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';
import { CVService } from '../../Services/CV.service';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomValidators } from '../../Validators/CustomValidators';

@Component({
  selector: 'app-cv-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDialogModule,
    CommonModule,
    MatTableModule,
    MatCardModule,
    MatIconModule
  ],
  templateUrl: './cv-form.component.html',
  styleUrls: ['./cv-form.component.scss']
})
export class CvFormComponent {
  cvForm!: FormGroup;
  isEditMode = false;
  cvId!: number;

  experienceColumns = ['companyName', 'city', 'companyField', 'actions'];
  experienceDataSource!: MatTableDataSource<any>;

  constructor(private fb: FormBuilder,
    private dialog: MatDialog,
    private cvService: CVService,
    private route: ActivatedRoute,
    private router: Router) {
  }

  get personalInformation() {
    return this.cvForm.get('personalInformation') as FormGroup;
  }

  get experienceInformation() {
    return this.cvForm.get('experienceInformation') as FormArray;
  }

  ngOnInit() {
    this.cvForm = this.fb.group({
      name: ['', [Validators.required, CustomValidators.noWhitespace]],
      personalInformation: this.fb.group({
        id: [0], // Default to 0 if not provided
        fullName: ['', [Validators.required, CustomValidators.noWhitespace]],
        cityName: ['', [Validators.required, CustomValidators.noWhitespace]],
        email: ['', [Validators.required, Validators.email]],
        mobileNumber: ['', [Validators.required, Validators.pattern(/^([0-9]{1,4})?([0-9]{10})$/)]],
      }),
      experienceInformation: this.fb.array([]),
    });
    this.experienceDataSource = new MatTableDataSource();

    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.cvId = params['id'];
        const navigation = this.router.getCurrentNavigation();
        if (navigation?.extras.state?.['cv']) {
          this.loadCVData(navigation.extras.state['cv']);
        } else {
          this.loadCV(this.cvId);
        }
      }
    });
  }

  private loadCVData(cv: any) {
    this.cvForm.patchValue({
      name: cv.name,
      personalInformation: cv.personalInformation
    });

    while (this.experienceInformation.length) {
      this.experienceInformation.removeAt(0);
    }

    cv.experienceInformation.forEach((exp: any) => {
      this.addExperience(exp);
    });

    this.experienceDataSource.data = this.experienceInformation.controls;
  }

  loadCV(id: number) {
    this.cvService.getCVById(id).subscribe({
      next: (cv) => {
        this.cvForm.patchValue({
          name: cv.name,
          personalInformation: cv.personalInformation
        });

        while (this.experienceInformation.length) {
          this.experienceInformation.removeAt(0);
        }

        cv.experienceInformation.forEach(exp => {
          this.addExperience(exp);
        });

        this.experienceDataSource.data = this.experienceInformation.controls;
      },
      error: (error) => console.error(error)
    });
  }

  onSubmit(): void {
    if (this.cvForm.valid) {
      if (this.isEditMode) {
        this.cvService.updateCV(this.cvId, {
          id: this.cvId,
          ...this.cvForm.value
        }).subscribe({
          next: () => this.router.navigate(['/cvs']),
          error: (error) => console.error(error)
        });
      } else {
        this.cvService.createCV(this.cvForm.value).subscribe({
          next: () => this.router.navigate(['/cvs']),
          error: (error) => console.error(error)
        });
      }
    } else {
      this.cvForm.markAllAsTouched();
    }
  }

  openExperienceDialog(experienceData: any = null): void {
    const dialogRef = this.dialog.open(ExperienceDialogComponent, {
      width: '600px',
      data: experienceData || {}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        if (experienceData) {
          this.updateExperience(result, experienceData);
        } else {
          this.addExperience(result);
        }
      }
    });
  }

  addExperience(experienceData: any): void {
    const experienceGroup = this.fb.group({
      id: [experienceData.id || 0], // Default to 0 if not provided
      companyName: [experienceData.companyName, Validators.required],
      city: [experienceData.city, Validators.required],
      companyField: [experienceData.companyField, Validators.required],
    });

    this.experienceInformation.push(experienceGroup);
    this.experienceDataSource.data = this.experienceInformation.controls;
  }

  updateExperience(updatedExperience: any, experienceData: any): void {
    const experienceIndex = this.experienceInformation.controls.findIndex(
      (control) => control.value === experienceData
    );

    if (experienceIndex !== -1) {
      this.experienceInformation.at(experienceIndex).patchValue(updatedExperience);
      this.experienceDataSource.data = this.experienceInformation.controls;
    }
  }

  onDeleteExperience(index: number): void {
    this.experienceInformation.removeAt(index);
    this.experienceDataSource.data = this.experienceInformation.controls;
  }

  goBack(): void {
    this.router.navigate(['/cvs']);
  }
}