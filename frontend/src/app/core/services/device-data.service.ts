// src/app/core/services/device-data.service.ts
import { Injectable }      from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable }      from 'rxjs';
import { map }            from 'rxjs/operators';

export interface DeviceDataDto {
  deviceDataId: number;
  deviceId:      number;
  timestamp:     string;
  value:         number;
  userId:        number;
  command:       string | null;
}

export interface AggregatedDeviceData {
  timestamp: string;
  avgValue:  number;
  minValue:  number;
  maxValue:  number;
}

@Injectable({ providedIn: 'root' })
export class DeviceDataService {
  private base = '/api/DeviceData';

  constructor(private http: HttpClient) {}

  /** Dados brutos paginados */
  getPage(
    deviceId: number, from: string, to: string,
    page = 1, pageSize = 50
  ): Observable<{ items: DeviceDataDto[]; page: number; pageSize: number; totalItems: number }> {
    const params = new HttpParams()
      .set('deviceId', deviceId.toString())
      .set('from',     from)
      .set('to',       to)
      .set('page',     page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<any>(`${this.base}`, { params })
      .pipe(map(resp => ({
        items: resp.items.map((d: any) => ({
          deviceDataId: d.DeviceDataId,
          deviceId:     d.DeviceId,
          timestamp:    d.Timestamp,
          value:        d.Value,
          userId:       d.UserId,
          command:      d.Command
        })),
        page:       resp.page,
        pageSize:   resp.pageSize,
        totalItems: resp.totalItems
      })));
  }

  /** Dados agregados para gráfico */
  getAggregated(
    deviceId: number, from: string, to: string, interval: 'Minute'|'Hour'|'Day'
  ): Observable<AggregatedDeviceData[]> {
    const params = new HttpParams()
      .set('deviceId', deviceId.toString())
      .set('from',     from)
      .set('to',       to)
      .set('interval', interval);

    return this.http.get<any[]>(`${this.base}/aggregate`, { params })
      .pipe(map(list => list.map(d => ({
        timestamp: d.Timestamp,
        avgValue:  d.AvgValue,
        minValue:  d.MinValue,
        maxValue:  d.MaxValue
      }))));
  }

  /** Última leitura */
  getLatest(deviceId: number): Observable<DeviceDataDto> {
    const params = new HttpParams().set('deviceId', deviceId.toString());
    return this.http.get<any>(`${this.base}/latest`, { params })
      .pipe(map(d => ({
        deviceDataId: d.DeviceDataId,
        deviceId:     d.DeviceId,
        timestamp:    d.Timestamp,
        value:        d.Value,
        userId:       d.UserId,
        command:      d.Command
      })));
  }
}
