import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { VersionComponent } from './version.component';

describe('VersionComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VersionComponent],
    }).compileComponents();
  });

  it('should create the component', () => {
    // Arrange
    const fixture = TestBed.createComponent(VersionComponent);

    // Act
    const component = fixture.componentInstance;

    // Assert
    expect(component).toBeTruthy();
  });

  it('should display the app name in the card title', async () => {
    // Arrange
    const fixture = TestBed.createComponent(VersionComponent);

    // Act
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;

    // Assert
    expect(compiled.querySelector('h5.card-title')?.textContent?.trim()).toBe(environment.appName);
  });

  it('should display the app version in the card text', async () => {
    // Arrange
    const fixture = TestBed.createComponent(VersionComponent);

    // Act
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;

    // Assert
    expect(compiled.querySelector('p.card-text')?.textContent?.trim()).toBe(`Version: ${environment.appVersion}`);
  });
});
