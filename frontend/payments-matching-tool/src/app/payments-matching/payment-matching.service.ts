import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PaymentMatchRow, ResolutionSide, ResultFilter, RunMatchResponse } from './models';

@Injectable({ providedIn: 'root' })
export class PaymentMatchingService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/matches`;

  runMatch(systemFile: File, providerFile: File): Observable<RunMatchResponse> {
    const formData = new FormData();
    formData.append('SystemFile', systemFile);
    formData.append('ProviderFile', providerFile);

    return this.http.post<RunMatchResponse>(`${this.baseUrl}/run`, formData);
  }

  getItems(batchId: string, filter: ResultFilter): Observable<PaymentMatchRow[]> {
    return this.http.get<PaymentMatchRow[]>(`${this.baseUrl}/${batchId}`, { params: { filter } });
  }

  resolve(itemId: string, resolutionSide: ResolutionSide): Observable<PaymentMatchRow> {
    return this.http.patch<PaymentMatchRow>(`${this.baseUrl}/items/${itemId}/resolve`, { resolutionSide });
  }
}
