import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { LoanService } from '../../../Services/loan-service';
import { LoanTableModel } from '../../../Models/Responses/Loans/loan-table-model';



@Component({
  selector: 'app-loans',
  standalone: true,
  imports: [CommonModule, MatTableModule],
  templateUrl: './get-all-loans.component.html',
})
export class LoansComponent implements OnInit {
  private loanService = inject(LoanService);

  loans: LoanTableModel[] = [];
  displayedColumns: string[] = [
    'loanAmount',
    'currentBalance',
    'applicant',
    'status',
  ];

  ngOnInit(): void {
    this.loanService.getAll().subscribe(data => {
      this.loans = data;
    });
  }
}
