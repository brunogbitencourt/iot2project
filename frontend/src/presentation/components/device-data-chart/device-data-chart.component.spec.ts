import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeviceDataChartComponent } from '../device-data-chart/device-data-chart.component';

describe('DeviceDataChart', () => {
  let component: DeviceDataChartComponent;
  let fixture: ComponentFixture<DeviceDataChartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeviceDataChartComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeviceDataChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
