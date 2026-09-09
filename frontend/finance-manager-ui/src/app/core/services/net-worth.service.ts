import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { NetWorth } from '../models/net-worth.model';

@Injectable({
  providedIn: 'root'
})
export class NetWorthService {

  private http = inject(HttpClient);

  private readonly apiUrl =
    'http://localhost:5228/api/NetWorth';

  getNetWorth(): Observable<NetWorth> {
    return this.http.get<NetWorth>(
      this.apiUrl
    );
  }
}