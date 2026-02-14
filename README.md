# Blazy - Nostalgic Social Media Blog Platform

A nostalgic social media blog platform inspired by the early 2000s internet era, built with ASP.NET Core 8.0 and MVC.

## ✧ Features ✧

- **Custom HTML Profiles**: Users can fully customize their profile pages with custom HTML, CSS, backgrounds, fonts, banners, images, and music
- **Nostalgic Design**: Early 2000s MySpace-style aesthetic with cute, playful elements
- **User Authentication**: ASP.NET Core Identity with Admin and User roles
- **Social Features**: Subscribe to blogs, like/dislike posts, and comment on content
- **Tag System**: Organize content with tags and discover new posts
- **Layered Architecture**: Clean separation of concerns with Core, Data, Repository, Services, and Web layers
- **Entity Framework Core**: SQL database with automatic migrations

## 🌟 Project Structure

```
Blazy/
├── Blazy.Core/          # Domain entities and DTOs
├── Blazy.Data/          # DbContext and database configuration
├── Blazy.Repository/    # Data access layer with repositories
├── Blazy.Services/      # Business logic layer
├── Blazy.Web/          # MVC web application
├── Blazy.Tests/        # Unit tests
└── Blazy.sln          # Solution file
```

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022/2023 or VS Code
- SQL Server (LocalDB or full SQL Server)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/blazy.git
   cd blazy
   ```

2. **Open the solution**
   ```bash
   dotnet restore Blazy.sln
   ```

3. **Configure the connection string**
   
   Edit `Blazy.Web/appsettings.json` and update the connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BlazyDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
   }
   ```

4. **Run the application**
   ```bash
   cd Blazy.Web
   dotnet run
   ```

5. **Access the application**
   Open your browser and navigate to `https://localhost:7001`

### Default Admin Account

- **Username**: admin
- **Password**: Admin123!

## 🎨 Customization

### Profile Customization

Users can customize their profiles with:
- Custom HTML content
- Custom CSS styles
- Background images
- Banner images
- Background music
- Custom fonts
- Accent colors

### MySpace-Style HTML

The platform supports barebones HTML, allowing users to create nostalgic, personalized profiles. Popular elements include:
- Marquee text
- Custom backgrounds
- Embedded music players
- Glitter and sparkles
- Custom fonts and colors

## 📝 API Endpoints

### Authentication
- `POST /Account/Register` - Register a new user
- `POST /Account/Login` - Login user
- `POST /Account/Logout` - Logout user

### Posts
- `GET /Post/Index/{id}` - View a post
- `GET /Post/Create` - Create post form
- `POST /Post/Create` - Create a new post
- `POST /Post/Like/{id}` - Like a post
- `POST /Post/Dislike/{id}` - Dislike a post
- `POST /Post/Delete/{id}` - Delete a post

### Blogs
- `GET /{username}` - View user's blog/profile
- `POST /Blog/Subscribe` - Subscribe to a user
- `POST /Blog/Unsubscribe` - Unsubscribe from a user

## 🏗️ Architecture

### Layered Architecture

1. **Blazy.Core**: Domain entities (User, Post, Comment, Subscription, Like, Dislike, Tag) and DTOs
2. **Blazy.Data**: DbContext, entity configurations, and database initialization
3. **Blazy.Repository**: Generic and specific repositories for data access
4. **Blazy.Services**: Business logic and service interfaces
5. **Blazy.Web**: MVC controllers, views, and static assets
6. **Blazy.Tests**: Unit tests for services and repositories

### Database Schema

The application uses the following entities with relationships:
- **User**: Main user entity with profile customization fields
- **Post**: Blog posts with tags, likes, dislikes, and comments
- **Comment**: Comments on posts
- **Subscription**: Many-to-many relationship for blog subscriptions
- **Like/Dislike**: User reactions to posts
- **Tag**: Tags for organizing content
- **PostTag/UserTag**: Junction entities for many-to-many relationships

## 🧪 Testing

Run the unit tests:

```bash
dotnet test Blazy.Tests/Blazy.Tests.csproj
```

## 🛠️ Technologies Used

- **.NET 8.0**: Latest .NET framework
- **ASP.NET Core MVC**: Web framework
- **Entity Framework Core 8.0**: ORM for database operations
- **ASP.NET Core Identity**: Authentication and authorization
- **SQL Server**: Database backend
- **xUnit**: Testing framework
- **Moq**: Mocking framework for tests

## 📄 License

This project is open source and available under the MIT License.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📞 Support

For questions or support, please open an issue on GitHub.

---

**Made with ♥ for the nostalgic internet era**

*Blazy - Your Space, Your Style ✧*