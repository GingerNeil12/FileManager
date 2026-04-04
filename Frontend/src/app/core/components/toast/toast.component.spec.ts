import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ToastComponent } from './toast.component';

describe('ToastComponent', () => {
  let fixture: ComponentFixture<ToastComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToastComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ToastComponent);
    fixture.componentRef.setInput('message', 'Test error message');
    fixture.detectChanges();
  });

  it('should create the component', () => {
    // Arrange / Act / Assert
    expect(fixture.componentInstance).toBeTruthy();
  });

  describe('message display', () => {
    it('should render the message in the toast body', () => {
      // Arrange
      fixture.componentRef.setInput('message', 'Unable to reach backend service.');
      fixture.detectChanges();

      // Act
      const toastBody = fixture.nativeElement.querySelector('.toast-body') as HTMLElement;

      // Assert
      expect(toastBody.textContent?.trim()).toBe('Unable to reach backend service.');
    });

    it('should update the message when the input changes', () => {
      // Arrange
      fixture.componentRef.setInput('message', 'First message');
      fixture.detectChanges();

      // Act
      fixture.componentRef.setInput('message', 'Updated message');
      fixture.detectChanges();
      const toastBody = fixture.nativeElement.querySelector('.toast-body') as HTMLElement;

      // Assert
      expect(toastBody.textContent?.trim()).toBe('Updated message');
    });
  });

  describe('dismiss', () => {
    it('should emit the dismissed event when the close button is clicked', () => {
      // Arrange
      let emitted = false;
      fixture.componentInstance.dismissed.subscribe(() => (emitted = true));
      const closeButton = fixture.nativeElement.querySelector('button.btn-close') as HTMLButtonElement;

      // Act
      closeButton.click();

      // Assert
      expect(emitted).toBe(true);
    });

    it('should emit dismissed only once per click', () => {
      // Arrange
      let emitCount = 0;
      fixture.componentInstance.dismissed.subscribe(() => emitCount++);
      const closeButton = fixture.nativeElement.querySelector('button.btn-close') as HTMLButtonElement;

      // Act
      closeButton.click();

      // Assert
      expect(emitCount).toBe(1);
    });
  });
});
