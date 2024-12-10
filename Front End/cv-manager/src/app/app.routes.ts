import { Routes } from '@angular/router';
import { CvFormComponent } from './components/cv-form/cv-form.component';
import { CvListComponent } from './components/cv-list/cv-list.component';

export const routes: Routes = [
    { path: '', redirectTo: 'cvs', pathMatch: 'full' },
    { path: 'cvs', component: CvListComponent },
    { path: 'cv/new', component: CvFormComponent },
    { path: 'cv/edit/:id', component: CvFormComponent }
  ];
