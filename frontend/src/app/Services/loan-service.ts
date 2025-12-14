import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../enviroment';
import { GetAllLoansResponse } from '../Models/Responses/Loans/get-all-loans-reponse';
import { LoanTableModel } from '../Models/Responses/Loans/loan-table-model';
import { LoanStatusEnum } from '../Models/Enums/LoanStatusEnum';


type LoansApiResponse = {
  loanModels: GetAllLoansResponse[];
};

@Injectable({ providedIn: 'root' })
export class LoanService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<LoanTableModel[]> {
    return this.http
      .get<LoansApiResponse>(`${environment.baseUrl}loans`)
      .pipe(
        map(res =>
          (res.loanModels ?? []).map(l => this.toTableModel(l))
        )
      );
  }

  private toTableModel(l: GetAllLoansResponse): LoanTableModel {
    return {
      loanAmount: l.amount ?? 0,
      currentBalance: l.currentBalance ?? 0,
      applicant: l.applicantName ?? '',
      status: this.mapStatus(l.status),
    };
  }

  private mapStatus(status: LoanStatusEnum | null): string {
    switch (status) {
      case LoanStatusEnum.Active:
        return 'Active';
      case LoanStatusEnum.None:
        return 'None';
      case LoanStatusEnum.Paid:
        return 'Paid';
      default:
        return 'Unknown';
    }
  }
}
