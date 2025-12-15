## Running the Backend


To run application
```sh
docker compose up --build
```

The Api has two controllers > Auth && Loan
> To access the endpoints from loan, the authorization is required. So, is necessary register a new user and login to receive a token

>Register Endpoint: http://localhost:5000/api/auth/register

>Login Endpoint: http://localhost:5000/api/auth/login

The LoanController has 4 endpoints: 

```http

GETALLLOANS -> https://localhost:5001/loan
CREATElOAN -> http://localhost:5000/loan
GETLOANBYID -> http://localhost:5000/loan/2002
PAYMENT -> http://localhost:5000/loan/1/payment

```

> Swagger
https://localhost:5001/loan/swagger


## 📬 API – Postman Collection

The backend API can be tested using Postman.

- 👉 [Download Postman Collection](./postman/LoanApi.postman_collection.json)

## Notes  

Feel free to modify the code as needed, but try to **respect and extend the current architecture**, as this is intended to be a replica of the Fundo codebase.
