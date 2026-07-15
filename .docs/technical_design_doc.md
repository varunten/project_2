# Technical Design Document

## Introduction

### Purpose
Stimulate a real time enterprise insurance application developed in a CMMI Level 3 compliant IT organization.

## System Architecture

## Technology Stack

## Database Design

### **Table:** `Users`

**Purpose**: stores all registered users of the IPMS platform under roles of `Customer`, `Admin`, `UnderWriter` and `Insurance Agent`     

**Columns:**    
| Column | Type | Constraints | Description |
| - | - | - | - |
| id | UUID | Primary Key | |
| first_name | VARCHAR | NOT NULL |  |
| middle_name | VARCHAR |  |  |
| last_name | VARCHAR |  |  |
| email | VARCHAR | NOT NULL, UNIQUE |  |
| password_hash | VARCHAR | NOT NULL |  |
| phone_number | VARCHAR | NOT NULL, UNIQUE |  |
| last_login_at | TIMESTAMP |  |  |
| deleted_at | TIMESTAMP |  |  |
| created_at | TIMESTAMP | NOT NULL |  |
| updated_at | TIMESTAMP | NOT NULL |  |


## Data Flow Diagrams

## Authentication and Authorization

## API Design

### **Endpoint**: `/auth/signup `        
**Purpose**: Signup new customers           
**Authentication**: None        
**Authorization**: None     
**Request Body**:   
```json
{
    "email": "",
    "password": "",
    "password_confirm": "",
    "phone_number": ""
}
```
**Success Response**:
```json
{
    "success": true,
    "message": "User created successfully"
}
```   
**Database Tables Affected**:   
| Table | Action |
| - | - |
| Users | INSERT |


## Error Handling

## Security

## Logging