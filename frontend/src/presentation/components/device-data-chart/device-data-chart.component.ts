// src/app/presentation/components/device-data-chart/device-data-chart.component.ts

import { Component, Input, OnChanges, ViewChild  } from '@angular/core';
import { BaseChartDirective }           from 'ng2-charts';
import 'chart.js/auto';
import 'chartjs-adapter-date-fns';
import { ChartType, ChartConfiguration, ChartOptions, TimeUnit } from 'chart.js';


// **Atenção:** estes tipos vêm do serviço device-data.service.ts, não do device.service.ts
import {
  DeviceDataService,
  AggregatedDeviceData
} from '../../../app/core/services/device-data.service';

@Component({
  selector: 'app-device-data-chart',
  standalone: true,
  imports: [BaseChartDirective],
  templateUrl: './device-data-chart.component.html',
  styleUrls: ['./device-data-chart.component.scss']
})
export class DeviceDataChartComponent implements OnChanges {
  /** ID do dispositivo a buscar */
  @Input() deviceId!: number;
  /** ISO string “YYYY-MM-DDThh:mm” */
  @Input() from!: string;
  @Input() to!: string;
  /** 'Minute' | 'Hour' | 'Day' */
  @Input() interval: 'Minute' | 'Hour' | 'Day' = 'Hour';
  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;

  public chartType: ChartType = 'line';

  public lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [
      { data: [], label: 'Média', fill: false },
      { data: [], label: 'Mínimo', fill: false },
      { data: [], label: 'Máximo', fill: false }
    ]
  };

  public lineChartOptions: ChartOptions<'line'> = {
  responsive: true,
  scales: {
    x: {
      type: 'time',
      time: {
        unit: this.interval.toLowerCase() as TimeUnit,
        displayFormats: {
          minute: 'HH:mm',
          hour:   'HH:mm',
          day:    'dd/MM'
        }
      }
    },
    y: {
      beginAtZero: false
    }
  }
};


  constructor(private svc: DeviceDataService) {}

ngOnChanges(): void {
  console.log('Inputs mudaram:', {
    deviceId: this.deviceId,
    from:     this.from,
    to:       this.to,
    interval:this.interval
  });
  if (this.deviceId && this.from && this.to) {
    this.svc
      .getAggregated(this.deviceId, this.from, this.to, this.interval)
      .subscribe(data => {
        console.log('Dados agregados recebidos:', data);
        this.updateChart(data);
      });
  }
  
}

  private updateChart(data: AggregatedDeviceData[]): void {
  this.lineChartData.labels    = data.map(d => d.timestamp);
  this.lineChartData.datasets[0].data = data.map(d => d.avgValue);
  this.lineChartData.datasets[1].data = data.map(d => d.minValue);
  this.lineChartData.datasets[2].data = data.map(d => d.maxValue);

  // --- NOVO: cálculo de min/max ---
  const allValues = data.flatMap(d => [d.minValue, d.maxValue]);
  const min = Math.min(...allValues);
  const max = Math.max(...allValues);

  // acessa usando ['y']
  if (this.chart && this.chart.options && this.chart.options.scales) {
    const scales = this.chart.options.scales;
    // e se existir o eixo 'y'
    if ('y' in scales && scales['y']) {
      scales['y']!.min = min;
      scales['y']!.max = max;
    }
  }
  this.chart?.update();
}

}
