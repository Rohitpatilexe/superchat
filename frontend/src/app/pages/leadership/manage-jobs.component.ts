
import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { LeadershipService } from '../../services/leadership.service';
import { VendorService } from '../../services/vendor.service'; // Re-using for type
import { Job, Vendor } from '../../models';
import { COUNTRIES } from '../../constants/countries';
import { Observable, startWith, map } from 'rxjs';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
 
@Component({
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    DatePipe,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatInputModule
  ],
  template: `
    <div class="space-y-6">
      <div class="p-6 bg-white rounded-lg shadow">
        <h2 class="text-xl font-semibold">Create New Job</h2>
        <form [formGroup]="jobForm" (ngSubmit)="createJob()" class="mt-4 space-y-4">
          <input formControlName="title" placeholder="Job Title" class="w-full px-3 py-2 border rounded">
          <textarea formControlName="description" placeholder="Job Description" class="w-full px-3 py-2 border rounded"></textarea>
 
          <mat-form-field appearance="outline" class="w-full">
            <input
              type="text"
              matInput
              [formControl]="countryControl"
              [matAutocomplete]="auto"
              placeholder="Select or type a country"
              (input)="getVendorsForCountry()"
              class="px-3 py-2 border rounded"
            >
            <mat-autocomplete #auto="matAutocomplete">
              <mat-option *ngFor="let country of filteredCountries | async" [value]="country">
                {{ country }}
              </mat-option>
            </mat-autocomplete>
          </mat-form-field>
 
          <input formControlName="expiryDate" type="date" class="w-full px-3 py-2 border rounded">
         
          <div *ngIf="availableVendors.length > 0">
            <h3 class="font-medium">Assign Vendors (from specified country)</h3>
            <div class="grid grid-cols-2 md:grid-cols-4 gap-2 mt-2">
              <label *ngFor="let vendor of availableVendors" class="flex items-center p-2 border rounded-md">
                <input type="checkbox" [value]="vendor.publicId" (change)="onVendorChange($event)">
                <span class="ml-2 text-sm">{{ vendor.companyName }}</span>
              </label>
            </div>
          </div>
         
          <button type="submit" [disabled]="jobForm.invalid" class="w-full px-4 py-2 text-white bg-blue-600 rounded disabled:bg-gray-400">Create Job</button>
        </form>
      </div>
 
      <div class="p-6 bg-white rounded-lg shadow">
        <h2 class="text-xl font-semibold">My Created Jobs</h2>
        <ul class="mt-4 space-y-2">
          <li *ngFor="let job of jobs" class="flex justify-between items-center p-3 bg-gray-50 rounded hover:bg-gray-100">
            <div>
              <p class="font-semibold">{{ job.title }}</p>
              <p class="text-sm text-gray-600">{{ job.country }} - Expires: {{ job.expiryDate | date:'shortDate' }}</p>
            </div>
            <a [routerLink]="['/leadership/jobs', job.publicId]" class="px-3 py-1 text-sm text-white bg-gray-700 rounded hover:bg-black">View Details</a>
          </li>
        </ul>
      </div>
    </div>
  `
})
export class ManageJobsComponent implements OnInit {
  jobs: Job[] = [];
  availableVendors: Vendor[] = [];
  jobForm: FormGroup;
  countries = COUNTRIES;
  countryControl = new FormControl('India');
  filteredCountries!: Observable<string[]>;
 
  constructor(private leadershipService: LeadershipService, private fb: FormBuilder) {
    this.jobForm = this.fb.group({
      title: ['', Validators.required],
      description: ['', Validators.required],
      country: this.countryControl,
      expiryDate: ['', Validators.required],
      assignedVendorPublicIds: [[]]
    });
  }
 
  ngOnInit() {
    this.loadJobs();
    this.filteredCountries = this.countryControl.valueChanges.pipe(
      startWith(''),
      map(value => this.filterCountries(value || ''))
    );
    this.getVendorsForCountry(); // Initial load for default country
  }
 
  private filterCountries(value: string): string[] {
    const filterValue = value.toLowerCase();
    return this.countries.filter(country =>
      country.toLowerCase().includes(filterValue)
    );
  }
 
  loadJobs() {
    this.leadershipService.getMyCreatedJobs().subscribe(data => this.jobs = data);
  }
 
  getVendorsForCountry() {
    const country = this.countryControl.value;
    if (country) {
      this.leadershipService.getVendorsByCountry(country).subscribe(data => this.availableVendors = data);
    }
  }
 
  onVendorChange(event: any) {
    const selectedIds = this.jobForm.get('assignedVendorPublicIds')?.value as string[];
    if (event.target.checked) {
      selectedIds.push(event.target.value);
    } else {
      const index = selectedIds.indexOf(event.target.value);
      if (index > -1) {
        selectedIds.splice(index, 1);
      }
    }
    this.jobForm.get('assignedVendorPublicIds')?.setValue(selectedIds);
  }
 
  createJob() {
    if (this.jobForm.invalid) return;
   
    this.leadershipService.createJob(this.jobForm.value).subscribe(() => {
      this.loadJobs();
      this.jobForm.reset({ country: 'India', assignedVendorPublicIds: [] });
      this.countryControl.setValue('India');
      this.getVendorsForCountry();
    });
  }
}
 
 
