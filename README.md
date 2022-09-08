# WhatSenderDesktop - Bulk WhatsApp Sender 
[![GPLv3 License](https://img.shields.io/badge/License-GPL%20v3-yellow.svg)](https://opensource.org/licenses/)


Desktop Application to send customised dynamic bulk WhatsApp Messages
WhatSender v1.0

### Installation
This is the c# .net desktop application software so you required following tools to open the source code.

-  Visual Studio (Download-> https://visualstudio.microsoft.com/downloads)

Follow below steps to build the project
-   Open the `.sln` file in the visual studio.
-   Build the solution and run it.

Document
Two ways to upload data into this application
1.  File Upload (Traditional)
2.  API integration (JSON)


### 1. File Upload
In the project folder you will get sample file.csv to upload into file section as described below.

Rules : 
-   First column should be the ‘phone’.
-   File should be in CSV in this version V1.0

Steps:
1.	Data Source selection
2.	Upload data using File
3.	Set the messages as per your requirement, You may use the extra columns during message generation like if you want the clientname in the message then write the {{clientname}} and the same thing for the file attachment.
 
### 2. API Integration

There will be two API to integrate WhatSender with your application.
- 	Data Fetch for Messages
- 	Status update to the Message

### Fetch Messages Data
- Method type : GET
- URL : https://xyz.anything

#### Sample Payload

```json
{
  "messages": [
    {
      "id": 234,
      "phone": "917405136746",
      "message": "This is an automated message",
      "attachment": "C:/filepathwillbehere/abc.pdf",
      "isMedia": false,
      "isDocument": true,
    },
    {
      "id": 235,
      "phone": "917405136746",
      "message": "This is an automated message2",
      "attachment": "C:/filepathwillbehere/abc2.pdf",
      "isMedia": false,
      "isDocument": true
    }
  ]
}
```

### Status Update to the Messages
- Method type : GET (Instead of POST)
- URL : https://xyz.anything?id=234&status=true&error=ifstatusisfalse

## Authors

- [@parthkanani](https://www.github.com/parthkanani)


## Support

For support, email parth.kanani@versionhash.com

Codecanyon Profile : https://codecanyon.net/user/parthkanani
