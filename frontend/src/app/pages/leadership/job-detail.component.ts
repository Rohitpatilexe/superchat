import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { LeadershipService } from '../../services/leadership.service';
import { JobDetail, EmployeeWithVendor, Vendor, ConversationDto } from '../../models';
import { MessagingService } from '../../services/messaging.service';
import { ChatModalComponent } from '../chat-modal.component';

// This interface helps in organizing the data for the template
interface VendorEmployeeGroup {
  vendorName: string;
  employees: EmployeeWithVendor[];
}

@Component({
  standalone: true,
  imports: [CommonModule, DatePipe, RouterModule, ChatModalComponent],
  template: `
    <div class="bg-gray-50 min-h-screen py-12 px-4 sm:px-6 lg:px-8">
      <div *ngIf="job" class="bg-white rounded-3xl shadow-2xl border border-gray-100 max-w-5xl mx-auto">
        
        <!-- Job Detail Container -->
        <div class="p-8 space-y-10">
          
          <!-- Job Header Section -->
          <div class="border-b-4 border-indigo-100 pb-6">
              <div class="flex items-start space-x-5">
                  <svg class="w-12 h-12 text-indigo-600 flex-shrink-0 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 13.25L13.25 21l-3.25-3.25L13.25 10l-3.25-3.25L21 13.25zM12 2v2M4 12H2m20 0h-2M12 20v2M5.636 5.636l1.414 1.414M16.95 16.95l1.414 1.414M5.636 18.364l1.414-1.414M16.95 7.05l1.414-1.414"></path>
                  </svg>
                  <div>
                      <h2 class="text-5xl font-extrabold text-gray-900 tracking-tight">{{ job.title }}</h2>
                      <p class="text-lg text-indigo-700 font-medium mt-2">{{ job.country }}</p>
                      <p class="text-sm text-gray-500 mt-1">Created by: <span class="font-semibold text-gray-700">{{ job.createdBy.firstName }} {{ job.createdBy.lastName }}</span></p>
                  </div>
              </div>
              <!-- Expiry Date -->
              <div class="mt-4 pt-4 border-t border-red-100/50 ml-[70px]"> <!-- Aligned with text block -->
                  <p class="text-base font-semibold text-red-700 flex items-center">
                      <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
                      Deadline: <span class="ml-1 font-bold">{{ job.expiryDate | date:'fullDate' }}</span>
                  </p>
              </div>
          </div>
          
          <!-- Description -->
          <div class="pt-6 border-t border-gray-100">
            <h3 class="text-2xl font-bold mb-4 text-gray-800 flex items-center">
              <svg class="w-6 h-6 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path></svg>
              Job Description
            </h3>
            <p class="text-gray-700 leading-relaxed p-5 bg-gray-50 rounded-xl border border-gray-200 shadow-inner">{{ job.description }}</p>
          </div>
          
          <!-- Assigned Vendors -->
          <div class="pt-6 border-t border-gray-100">
            <h3 class="text-2xl font-bold mb-4 text-gray-800 flex items-center">
              <svg class="w-6 h-6 mr-2 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20v-2c0-.656-.126-1.283-.356-1.857M15 11H8V4a1 1 0 012-2h2a1 1 0 012 2v7M15 11h2m-2 0h-2m-4-7h10"></path></svg>
              Assigned Vendors ({{ job.assignedVendors.length }})
            </h3>
            <ul class="grid grid-cols-1 sm:grid-cols-2 gap-4 mt-4">
              <li *ngFor="let vendor of job.assignedVendors" class="flex justify-between items-center p-4 bg-blue-50 rounded-xl border-l-4 border-blue-400 shadow-md">
                  <span class="font-medium text-gray-900">{{ vendor.companyName }}</span>
                  <span 
                      [class.bg-green-100]="vendor.status === 'Active'" 
                      [class.text-green-700]="vendor.status === 'Active'" 
                      [class.bg-red-100]="vendor.status !== 'Active'" 
                      [class.text-red-700]="vendor.status !== 'Active'" 
                      class="text-xs font-bold p-1 px-3 rounded-full uppercase tracking-wider"
                  >
                      {{ vendor.status }}
                  </span>
              </li>
            </ul>
          </div>

          <!-- Submitted Employees -->
          <div class="pt-6 border-t border-gray-100">
            <h3 class="text-2xl font-bold mb-6 text-gray-800 flex items-center">
              <svg class="w-6 h-6 mr-2 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354l-7 7m0 0l7 7m-7-7h18"></path></svg>
              Submitted Candidates ({{ job.submittedEmployees.length }})
            </h3>
            
            <div *ngIf="!employeesByVendor.length" class="text-gray-500 mt-2 p-5 bg-yellow-50 rounded-xl border border-yellow-300">
                <p class="font-medium text-yellow-800 flex items-center">
                    <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
                    No candidates have been submitted for this job yet.
                </p>
            </div>
            
            <div class="space-y-6">
              <div *ngFor="let group of employeesByVendor" class="p-6 bg-gray-100 rounded-xl shadow-inner border border-purple-200">
                <h4 class="font-extrabold text-xl text-purple-700 border-b-2 border-purple-300 pb-3 mb-4 flex items-center">
                    <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h4m0 0v4m0-4L10 14"></path></svg>
                    Vendor: {{ group.vendorName }} ({{ group.employees.length }} Candidates)
                </h4>
                <ul class="space-y-2">
                  <li *ngFor="let emp of group.employees" class="p-3 bg-white rounded-lg hover:bg-purple-50 transition duration-150 flex justify-between items-center border border-gray-200">
                      <div class="flex flex-col">
                          <a [routerLink]="['/leadership/employees', emp.publicId]" class="font-semibold text-lg text-gray-900 hover:text-purple-600 transition duration-150">
                              {{ emp.firstName }} {{ emp.lastName }}
                          </a>
                          <span class="text-sm text-gray-500">{{ emp.jobTitle || 'Job Title N/A' }}</span>
                      </div>
                      <a [routerLink]="['/leadership/employees', emp.publicId]" class="text-sm font-bold text-purple-600 hover:text-purple-800 flex items-center transition duration-150">
                          Review Profile
                          <svg class="w-4 h-4 ml-1" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg>
                      </a>
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
    
      <!-- Loading State -->
      <div *ngIf="isLoading" class="text-center p-12 max-w-xl mx-auto bg-white rounded-xl shadow-lg border-t-4 border-indigo-400">
        <p class="text-xl font-medium text-indigo-600 flex items-center justify-center">
            <svg class="animate-spin h-5 w-5 mr-3 inline text-indigo-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            Loading job details...
        </p>
      </div>
      
      <!-- Error State (Implicitly handled by !job and !isLoading) -->
      <div *ngIf="!job && !isLoading" class="text-center p-12 max-w-xl mx-auto bg-white rounded-xl shadow-lg border-l-4 border-red-500">
        <p class="text-xl font-medium text-red-600 flex items-center justify-center">
            <svg class="w-6 h-6 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
            Job Not Found
        </p>
        <p class="mt-2 text-gray-600">The requested job details could not be found or loaded.</p>
      </div>
      <app-chat-modal *ngIf="selectedConversation" 
                    [conversation]="selectedConversation" 
                    (closeModal)="closeChat()">
      </app-chat-modal>
    </div>
  `
})
export class JobDetailComponent implements OnInit {
  job: JobDetail | null = null;
  isLoading = true;
  employeesByVendor: VendorEmployeeGroup[] = [];
  selectedConversation: ConversationDto | null = null;

  constructor(
    private route: ActivatedRoute,
    private leadershipService: LeadershipService,
    private messagingService: MessagingService
  ) {}

  ngOnInit() {
    const publicId = this.route.snapshot.paramMap.get('id');
    if (publicId) {
      this.leadershipService.getJobDetails(publicId).subscribe({
        next: (data) => {
          this.job = data;
          this.groupEmployeesByVendor(data.submittedEmployees);
          this.isLoading = false;
        },
        error: () => this.isLoading = false
      });
    } else {
      this.isLoading = false;
    }
  }

  private groupEmployeesByVendor(employees: EmployeeWithVendor[]) {
    if (!employees) {
      this.employeesByVendor = [];
      return;
    }
    const groups = new Map<string, EmployeeWithVendor[]>();
    employees.forEach(employee => {
      const vendorName = employee.vendorCompanyName;
      if (!groups.has(vendorName)) {
        groups.set(vendorName, []);
      }
      groups.get(vendorName)!.push(employee);
    });
    this.employeesByVendor = Array.from(groups.entries()).map(([vendorName, employees]) => ({
      vendorName,
      employees
    }));
  }

  openChatWithVendor(vendor: Vendor): void {
    if (!this.job) return;
    this.messagingService.startConversation(this.job.publicId, vendor.publicId).subscribe(response => {
      this.selectedConversation = {
        conversationPublicId: response.conversationPublicId,
        jobTitle: this.job?.title || '',
        participantPublicId: vendor.publicId,
        participantName: vendor.companyName
      };
    });
  }

  closeChat(): void {
    this.selectedConversation = null;
  }
}
