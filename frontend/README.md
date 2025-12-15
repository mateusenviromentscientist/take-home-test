## 🖥️ Frontend

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) **version 19.1.6**.

---

## 🔐 Prerequisites

Before accessing the frontend, make sure that:

1. A user has been **registered** using the **Auth API**
2. At least one **loan** has been created in the backend  
   (via **Postman** or **Swagger**)
3. The backend API is running
4. Create a Loan in the backend to check in the table

---

## 🚪 Application Flow

- The initial page is the **Login** screen
- Enter a valid **email** and **password**
- Upon successful authentication:
  - The user is redirected to the **Loans page**
  - A table is displayed with loan information retrieved from the backend API

---

## ▶️ Running the Frontend

Navigate to the `src` folder and install dependencies:

```sh
npm install

Start the development server:

npm run start


Open the application in your browser:

http://localhost:4200/