# Stock-Lite

Stock-Lite is a stock optimization system designed to help purchasers predict when and how much to order for their products.

The system aims to reduce:
- Out-of-stock situations (lost sales)
- Overstock scenarios (excess capital tied up in inventory)

This project was built to help small to medium enterprises improve purchasing decisions using structured forecasting and supply chain logic. It also serves as a practical implementation of applied supply chain domain knowledge.

## 🚀 Features
📅 Scheduler
- Import data into the system
- Perform automatic daily calculations
- Generate updated purchasing recommendations

👤 User Management
- Create users
- Edit users
- Delete users
- Secure login per user

📦 Product Management

For each product, the system provides:
- Insight into master data
- Forecasted sales expectations
- Historical sales overview
- Recommended order quantities (daily / weekly)

## 🏁 Getting Started

For installation and setup instructions, see: [docs/getting-started.md](docs/getting-started.md)

📖 Usage

For API usage and technical interaction details, see: [docs/api.md](docs/api.md)

🏗 Project Structure

📦Stock-Lite  
 ┣ 📂API  
 ┣ 📂Application  
 ┣ 📂Domain  
 ┣ 📂Infrastructure  
 ┣ 📂client  
 ┗ 📂docs

Folder Overview
- API – Application entry point and HTTP endpoints
- Application – Business logic and use cases
- Domain – Core entities and domain models
- Infrastructure – Database access and external integrations
- client – Frontend application
- docs – Project documentation

For more detailed information, see:
- [docs/architecture.md](docs/architecture.md) – System architecture explanation
- [docs/database.md](docs/database.md) – Database structure and stored procedures

🎯 Project Goal

Stock-Lite was created to:
- Support SMEs in making smarter purchasing decisions
- Prevent costly stock imbalances
- Translate supply chain expertise into practical software

📜 License

This project is licensed under a proprietary, view-only license. See the [LICENSE.md](LICENSE.md) for details.