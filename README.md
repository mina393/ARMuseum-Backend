ARMuseum Backend: Where History Meets the Future
Step into the past, reimagined for the future. ARMuseum is an innovative mobile application designed to bring the wonders of Egyptian museums to a global audience through immersive virtual tours. By fusing ancient history with cutting-edge technology, the app offers an experience that is both educational and breathtaking.

This repository contains the powerful, secure, and scalable ASP.NET Core backend that serves as the backbone for the entire ARMuseum experience, driving everything from ticket sales to groundbreaking AI reconstructions.

✨ Core Features
🎟️ Virtual Ticketing System: A complete and secure e-commerce solution that allows users to seamlessly purchase tickets for virtual museum tours. The system is fully integrated with the Paymob payment gateway for reliable and secure transaction processing.

🗿 Augmented Reality (AR) Integration: The API delivers the 3D models and data needed to bring ancient statues to life. Users can place and view artifacts in their own space through their mobile camera, creating a deeply personal and interactive connection with history.

🤖 AI-Powered Human Reconstruction: A groundbreaking feature that uses AI models to analyze statues and generate a stunning visual representation of historical figures as they might have appeared in life. This adds an unparalleled layer of educational depth and emotional engagement.

🔐 Secure User Authentication: A robust system for user registration, login (including social logins via Facebook), profile management, and password recovery, all secured using JWT (JSON Web Tokens) to protect user data and API endpoints.

⏳ Real-Time Usage Tracking: Leverages SignalR to monitor a user's time within the virtual experience, ensuring ticket validity and managing session duration dynamically for a smooth user experience.

📊 Admin Dashboard API: A dedicated set of endpoints for administrators to manage all aspects of the application, including museum content, ticket prices, user accounts, and viewing key statistics on sales and revenue.


🛠️ Tech Stack
Framework: ASP.NET Core 8

Database: SQL Server

ORM: Entity Framework Core 8

Authentication: ASP.NET Core Identity, JWT (JSON Web Tokens)

Real-time Communication: SignalR

Payment Gateway: Paymob API Integration

API Documentation: Swagger / OpenAPI

