# Photo Organizer

Photo Organizer is a serverless API that integrates photos with OneDrive. Designed primarily for use with iOS shortcuts, it can be easily implemented in other environments as well. By automating photo backups via simple HTTP API calls, this project saves users time and simplifies their photo management process.

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Installation and Deployment](#installation-and-deployment)
- [Configuration](#configuration)
- [API Documentation](#api-documentation)
- [Client Integration](#client-integration)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Serverless Architecture:** Built on AWS using a cloud-native deployment model.
- **OneDrive Integration:** Uses .NET's HttpClient to communicate directly with OneDrive APIs.
- **iOS Shortcuts Compatibility:** Easily trigger photo uploads and album management using Siri Shortcuts.
- **Automated Photo Backup:** Simplifies the backup process by handling authentication, upload, and album management via API endpoints.

## Tech Stack

- **Backend:** .NET (C#)  
- **Infrastructure:** AWS (deployed via a `template.yml` file)  
- **OneDrive Integration:** Handled using .NET's built-in `HttpClient` (no additional OneDrive libraries required)

## Installation and Deployment

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/dbrdak/photo-organizer.git
   cd photo-organizer
   ```

2. **Configure Deployment:**
   - Update the `template.yml` file with your deployment preferences.
   - Add the required environment variables to AWS Systems Manager.

3. **Deploy to AWS:**
   - Deploy the project to AWS using your preferred deployment method (e.g., AWS CLI, SAM CLI, or via the AWS Console using the provided CloudFormation template).

4. **Client Setup:**
   - Configure your iOS Shortcuts (or other client-side tool) to communicate with the deployed API endpoints.
   - Use Siri and shortcut automations to trigger these shortcuts for a seamless photo backup experience.

## Configuration

The following environment variables must be set in AWS Systems Manager (or your configuration management system):

- `/photo-organizer/microsoft/client-id`
- `/photo-organizer/microsoft/client-secret`
- `/photo-organizer/microsoft/drive-id`
- `/photo-organizer/microsoft/redirect-uri`
- `/photo-organizer/microsoft/scopes`

These variables are used for authenticating and communicating with the OneDrive API.

## API Documentation

The API exposes the following endpoints. Below is a summary of each endpoint along with its HTTP method, URL, and a brief description.

| Endpoint | Method | URL | Description |
|----------|--------|-----|-------------|
| Login | GET | {{base_url}}/microsoft/login | Logs in via Microsoft. The obtained token is automatically refreshed each day at 9:00 PM (when deployed using AWS with the default template.yml). |
| Upload Photo | POST | {{base_url}}/photos/new?name&extension&user | Uploads a photo file to the currently chosen album. Requires query parameters: name, extension, and user. The photo file is sent in the request body (file mode). |
| Set Album | POST | {{base_url}}/photos/new?name | Sets the current album by name. The album name is provided as a query parameter (name). |
| Reset Album | POST | {{base_url}}/photos/new | Resets the current album to the default album (Uncategorized). |

Note: Replace {{base_url}} with the actual base URL of your deployed API.

## Client Integration

While the backend provides all necessary endpoints, the client-side is left for you to set up. In my implementation, I used iOS Shortcuts with automation and Siri to trigger API calls. You can configure your shortcuts to:

- Initiate a login flow via the Login endpoint.
- Upload photos seamlessly using the Upload Photo endpoint.
- Manage albums (set or reset) using the corresponding endpoints.

This flexibility allows you to integrate Photo Organizer into any workflow that supports HTTP requests.

## Contributing

Contributions are welcome! If you would like to improve or extend the functionality of Photo Organizer, feel free to fork the repository and submit a pull request. Please ensure that your changes are well documented and include any necessary tests.

## License

This project is open source and available under the MIT License.

Happy organizing!
