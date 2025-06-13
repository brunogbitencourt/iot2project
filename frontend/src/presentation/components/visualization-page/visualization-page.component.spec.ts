import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VisualizationPageComponent  } from './visualization-page.component';

describe('VisualizationPage', () => {
  let component: VisualizationPageComponent ;
  let fixture: ComponentFixture<VisualizationPageComponent >;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VisualizationPageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VisualizationPageComponent );
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
