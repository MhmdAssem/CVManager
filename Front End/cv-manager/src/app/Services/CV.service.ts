// cv.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CV } from '../Interfaces/CV.interface';

@Injectable({
  providedIn: 'root'
})
export class CVService {
  private apiUrl = 'https://localhost:44371/api/cv';

  constructor(private http: HttpClient) {}

  getAllCVs(): Observable<CV[]> {
    return this.http.get<CV[]>(this.apiUrl);
  }

  getCVById(id: number): Observable<CV> {
    return this.http.get<CV>(`${this.apiUrl}/${id}`);
  }

  createCV(cv: CV): Observable<CV> {
    return this.http.post<CV>(this.apiUrl, cv);
  }

  updateCV(id: number, cv: CV): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, cv);
  }

  deleteCV(id: number): Observable<string> {
    return this.http.delete<string>(`${this.apiUrl}/${id}`);
  }
}