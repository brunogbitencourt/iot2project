import { Component, OnInit }       from '@angular/core';
import { CommonModule }            from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { DeviceDataChartComponent } from '../device-data-chart/device-data-chart.component';
import { DeviceService, DeviceDto } from '../../../app/core/services/device.service';

@Component({
  selector: 'app-visualization-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DeviceDataChartComponent
  ],
  templateUrl: './visualization-page.component.html',
  styleUrls: ['./visualization-page.component.scss']
})
export class VisualizationPageComponent implements OnInit {
  form: FormGroup;
  intervals = ['Minute','Hour','Day'] as const;
  devices: DeviceDto[] = [];

  constructor(
    private fb: FormBuilder,
    private deviceSvc: DeviceService
  ) {
    const now    = new Date();
    const before = new Date(now.getTime() - 3600*1000);
    this.form = this.fb.group({
      deviceId: [null],                              // <<< novo
      from:     [before.toISOString().slice(0,16)],
      to:       [now   .toISOString().slice(0,16)],
      interval: ['Hour']
    });
  }

  ngOnInit(): void {
  this.deviceSvc.getAll().subscribe(list => {
    console.log('Dispositivos recebidos do back:', list);
    this.devices = list;
    if (list.length) {
      this.form.patchValue({ deviceId: list[0].deviceId });
    }
  }, err => {
    console.error('Erro ao buscar dispositivos', err);
  });
}

}
