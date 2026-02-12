# Bluetooth Peer-to-Peer Messaging (RFCOMM)

## 📌 Overview

This project implements a lightweight peer-to-peer Bluetooth messaging application using the RFCOMM protocol. It enables short-range communication between devices without requiring internet connectivity.

The system was developed using C# and WPF, providing real-time device discovery, connection handling, and asynchronous message exchange through Bluetooth serial communication.

---

## 🎯 Objectives

* Design a device-to-device Bluetooth chat system
* Implement asynchronous message transmission
* Provide intuitive desktop UI
* Enable secure device discovery and connection
* Demonstrate signaling & communication principles

---

## 🛠 Technologies Used

* **.NET 8**
* **C#**
* **WPF (XAML UI)**
* **32feet.NET Bluetooth Library**
* **RFCOMM Protocol**

---

## 🏗 System Architecture

Application Flow:

```
WPF UI
  ↓
Application Logic
  ↓
Bluetooth RFCOMM Communication
  ↓
Peer Device
```

The system uses stream-based communication and async programming to maintain responsiveness.

---

## ⚙️ Features

* Device discovery
* Connect/Disconnect handling
* Username-based session start
* Real-time text messaging
* Asynchronous communication
* Stable RFCOMM connection

---

## ▶️ How to Run

1. Install .NET 8 SDK
2. Clone repository

```
git clone <repo-url>
```

3. Open `.sln` or `.csproj` in Visual Studio
4. Restore NuGet packages
5. Run application on two Bluetooth-enabled devices
6. Scan → Connect → Chat

---

## 📊 Performance (Observed)

* Connection Time: **2–3 seconds**
* Message Success Rate: **100%**
* Range: **10–15 meters**
* Supported Nodes: **2**

---

## ⚠️ Limitations

* Single connection only
* Windows-only support
* No encryption
* No file transfer

---

## 🚀 Future Improvements

* Message encryption
* Multi-node mesh support
* Cross-platform version
* File sharing
