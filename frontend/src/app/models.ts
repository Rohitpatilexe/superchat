// src/app/models.ts

// Represents a user (Admin or Leadership)
export interface User {
  publicId: string;
  email: string;
  firstName: string;
  lastName: string;
}

// Represents a vendor company
export interface Vendor {
  publicId: string;
  companyName: string;
  contactEmail: string;
  country: string;
  status: string;
}

// Represents an employee submitted by a vendor
export interface Employee {
  publicId: string;
  firstName: string;
  lastName: string;
  jobTitle?: string;
}

export interface EmployeeWithVendor extends Employee {
  vendorCompanyName: string;
}

export interface EmployeeDetail extends EmployeeWithVendor {
  createdAt: string; // ISO date string
  resumeDownloadUrl?: string;
}

// Represents a job listing (summary view)
export interface Job {
  publicId: string;
  title: string;
  country: string;
  createdAt: string; // ISO date string
  expiryDate: string; // ISO date string
  daysRemaining: number;
}

// Represents the detailed view of a job
export interface JobDetail extends Job {
  description: string;
  createdBy: User;
  assignedVendors: Vendor[];
  submittedEmployees: EmployeeWithVendor[];
}

// --- Messaging Models ---

// Represents a single message in a conversation
export interface MessageDto {
  id: number;
  content: string;
  sentAt: string; // ISO date string
  senderPublicId: string;
  senderName: string;
}

// Represents a conversation in a list (for an inbox view)
export interface ConversationDto {
  conversationPublicId: string;
  jobTitle: string;
  participantPublicId: string;
  participantName: string;
  lastMessage?: string;
  lastMessageTimestamp?: string; // ISO date string
}