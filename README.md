# 🌍 Travel Agency Web Application

## 🏗️ Architecture & Technologies

- **ASP.NET MVC** architecture (Model–View–Controller)  
- **C#** backend logic  
- **HTML, CSS, JavaScript** frontend  
- **Persistent data storage** using text files (TXT format)  
- **Authentication and Authorization** with user roles (Admin, Manager, Tourist)  
- **Session management** and an additional **cookie** for “remember me” functionality  
- **Repository** and **Service** layers for data access and business logic  
- **Enum types** used for arrangement type, transport type, and accommodation type  

---

## 👥 User Roles & Functionalities

### 🧳 Tourist
- Register and log in  
- Browse and search available travel arrangements  
- Make and cancel reservations  
- View all personal reservations (active, canceled, completed)  
- Leave comments and ratings for accommodations after returning from a trip  
- Edit personal profile  

### 🧑‍💼 Manager
- Create, modify, view, and logically delete travel arrangements  
- Manage accommodations and accommodation units  
- Review and approve/reject comments from tourists  
- View all reservations related to their own arrangements  

### 🛠️ Administrator
- View all system users  
- Search and filter users by role, name, or surname  
- Register new managers  

---

## 💾 Data Persistence

All application data (users, arrangements, accommodations, reservations, comments, etc.)  
are **stored in txt files** for simplicity and transparency.  
No external databases are used.

---

## 🔐 Authentication Details

- Login system based on user roles (Admin / Manager / Tourist)  
- Session-based authentication  
- Additional **cookie** for automatic login (“remember me” feature)  

---

## 🎨 UI and Design

The user interface is built using HTML, CSS, and basic JavaScript.  
Each page is styled to ensure clarity and a pleasant user experience.  
