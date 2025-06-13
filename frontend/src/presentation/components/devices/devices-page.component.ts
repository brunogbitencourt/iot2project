// src/presentation/components/devices/devices-page.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule }      from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { DevicesApiService, DeviceDto } from '../../../infrastructure/http/DevicesApiService';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-devices-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './devices-page.component.html',
  styleUrls: ['./devices-page.component.scss']
})
export class DevicesPageComponent implements OnInit {
  form: FormGroup;
  devices: DeviceDto[] = [];
  loading = false;
  editingId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private api: DevicesApiService
  ) {
    this.form = this.fb.group({
      UserId:        [null, Validators.required],
      ConnectedPort: ['', Validators.required],
      Name:          ['', Validators.required],
      Type:          ['', Validators.required],
      Category:      ['', Validators.required],
      Unit:          ['', Validators.required],
      MqttTopic:     ['', Validators.required],
      KafkaTopic:    ['', Validators.required],
    });
  }

  ngOnInit() {
    console.log('=> DevicesPageComponent init');
    this.loadDevices();
  }

  loadDevices() {
    console.log('=> loadDevices');
    this.loading = true;
    this.api.getAll()
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: list => {
          console.log('Fetched devices:', list);
          this.devices = list;
        },
        error: err => console.error('Erro fetching devices:', err)
      });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    const dto = this.form.value;
    const op$ = this.editingId != null
      ? this.api.update(this.editingId, dto)
      : this.api.create(dto);

    op$
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: res => {
          console.log('Save success:', res);
          this.resetForm();
          this.loadDevices();
        },
        error: err => console.error('Save error:', err)
      });
  }

  edit(device: DeviceDto) {
    this.editingId = device.DeviceId;
    this.form.setValue({
      UserId:        device.UserId,
      ConnectedPort: device.ConnectedPort,
      Name:          device.Name,
      Type:          device.Type,
      Category:      device.Category,
      Unit:          device.Unit,
      MqttTopic:     device.MqttTopic,
      KafkaTopic:    device.KafkaTopic,
    });
  }

  delete(id: number) {
    if (!confirm('Confirma exclusão deste dispositivo?')) return;
    this.api.delete(id).subscribe(() => this.loadDevices());
  }

  resetForm() {
    this.editingId = null;
    this.form.reset();
  }
}
