import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { NetWorth } from '../models/net-worth.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class NetWorthService {

  private http = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiUrl}/NetWorth`;

  getNetWorth(): Observable<NetWorth> {
    return this.http.get<NetWorth>(
      this.apiUrl
    );
  }
}