import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { CV } from '../../Interfaces/CV.interface';
import { CVService } from '../../Services/CV.service';

@Component({
  selector: 'app-cv-list',
  standalone: true,
  templateUrl: './cv-list.component.html',
  styleUrls: ['./cv-list.component.scss'],
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
})
export class CvListComponent implements OnInit {
  displayedColumns: string[] = ['name', 'fullName', 'email', 'actions'];
  dataSource: MatTableDataSource<CV>;
  isLoading = false;
  cvService!: CVService;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private injector: Injector,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.dataSource = new MatTableDataSource<CV>();
  }

  ngOnInit(): void {
    this.cvService = this.injector.get(CVService);
    this.loadCVs();
  }

  loadCVs(): void {
    this.isLoading = true;
    this.cvService.getAllCVs().subscribe({
      next: (cvs) => {
        this.dataSource.data = cvs;
        if (this.paginator) {
          this.dataSource.paginator = this.paginator;
        }
        if (this.sort) {
          this.dataSource.sort = this.sort;
        }
      },
      error: (error) => {
        this.snackBar.open('Error loading CVs', 'Close', { duration: 3000 });
        console.error(error);
      },
      complete: () => (this.isLoading = false),
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  editCV(id: number): void {
    this.cvService.getCVById(id).subscribe({
      next: (cv) => {
        this.router.navigate(['/cv/edit', id], { state: { cv } });
      },
      error: (error) => {
        this.snackBar.open('Error loading CV for edit', 'Close', { duration: 3000 });
        console.error(error);
      }
    });
  }

  deleteCV(id: number): void {
    if (confirm('Are you sure you want to delete this CV?')) {
      this.cvService.deleteCV(id).subscribe({
        next: (response) => {
          this.loadCVs();
          this.snackBar.open('CV deleted successfully', 'Close', {
            duration: 3000,
          });

        },
        error: (error) => {
          this.snackBar.open('Error deleting CV', 'Close', { duration: 3000 });
          console.error(error);
        },
      });
    }
  }

  createNewCV(): void {
    this.router.navigate(['/cv/new']);
  }
}