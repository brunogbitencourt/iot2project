import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule }    from '@angular/common';
import { DeviceService, DeviceDto } from '../../../app/core/services/device.service';
import { DeviceDataService, DeviceDataDto } from '../../../app/core/services/device-data.service';
import { interval, Subscription, forkJoin, of } from 'rxjs';
import { switchMap, catchError } from 'rxjs/operators';

interface DeviceWithLast {
  device: DeviceDto;
  last?: DeviceDataDto;
  error?: string;
}

@Component({
  selector: 'app-industrial-plant',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './industrial-plant.component.html',
  styleUrls: ['./industrial-plant.component.scss']
})
export class IndustrialPlantComponent implements OnInit, OnDestroy {
  devicesWithLast: DeviceWithLast[] = [];
  private pollingSub!: Subscription;

  constructor(
    private deviceSvc: DeviceService,
    private dataSvc: DeviceDataService
  ) {}

  ngOnInit(): void {
    // Primeiro carrega a lista de devices
    this.deviceSvc.getAll().pipe(
      catchError(err => {
        console.error('Erro ao buscar dispositivos:', err);
        return of([] as DeviceDto[]);
      })
    )
    .subscribe(list => {
      this.devicesWithLast = list.map(d => ({ device: d }));
      // Inicia o polling a cada 2s
      this.pollingSub = interval(2000)
        .pipe(
          switchMap(() => this.loadAllLastValues())
        )
        .subscribe();
    });
  }

  /** Faz forkJoin de todos os getLatest e atualiza o array */
  private loadAllLastValues() {
    if (this.devicesWithLast.length === 0) {
      return of(null);
    }

    // Para cada dispositivo, cria um Observable de getLatest
    const observables = this.devicesWithLast.map(item =>
      this.dataSvc.getLatest(item.device.deviceId).pipe(
        catchError(err => {
          console.error(`Erro no latest de ${item.device.deviceId}:`, err);
          return of(null as DeviceDataDto|null);
        })
      )
    );

    // Executa todos em paralelo
    return forkJoin(observables).pipe(
      switchMap(results => {
        // Atualiza cada entry com o resultado correspondente
        results.forEach((data, idx) => {
          if (data) {
            this.devicesWithLast[idx].last = data;
            this.devicesWithLast[idx].error = undefined;
          } else {
            this.devicesWithLast[idx].error = 'Sem dados';
          }
        });
        return of(null);
      })
    );
  }

  ngOnDestroy(): void {
    this.pollingSub?.unsubscribe();
  }
}
