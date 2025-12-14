import { LoanStatusEnum } from "../../Enums/LoanStatusEnum";

export interface GetAllLoansResponse{
    id: number,
    currentBalance: number | null,
    amount: number | null,
    applicantName: string | null,
    status: LoanStatusEnum | number
}