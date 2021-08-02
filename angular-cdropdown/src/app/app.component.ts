// app.component.ts

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { CountryService } from '../app/Services/select.service';
import { Country } from '../app/Classes/country-vm';
import { State } from '../app/Classes/state-vm';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  loginForm: FormGroup;
  name = 'Angular 5';
  countries: Country[];
  states: State[];

  constructor(private fb: FormBuilder, private selectService: CountryService) {
  }

  ngOnInit(): void {
    this.initForm();
    this.countries = this.selectService.getCountries();
  }

  onSelect(countryid) {
    this.states = this.selectService.getStates().filter((item) => item.countryid == countryid);
  }
  initForm(): void {

    this.loginForm = this.fb.group({
      email: ['', [Validators.required,
      Validators.pattern('^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$')]],
      password: ['', Validators.required],
      country: ['', Validators.required],
      state: ['', Validators.required]

    });

  }

  isValidInput(fieldName): boolean {
    return this.loginForm.controls[fieldName].invalid &&
      (this.loginForm.controls[fieldName].dirty || this.loginForm.controls[fieldName].touched);
  }

  login(): void {
    console.log(this.loginForm.value);
  }

  updateCountry(id: number) {

    alert(id);
  }

  CheckAllOptions() {
    if (this.countries.every(val => val.checked == true))
      this.countries.forEach(val => { val.checked = false });
    else
      this.countries.forEach(val => { val.checked = true });
  }
  getSelected() {

    debugger;
    this.countries.forEach(val => {
      if (val.checked == true) {
        alert(val.id)
      }
    });


  }

  checkIfAllSelected(values: any) {
    //alert(values.currentTarget.checked);
  }
}