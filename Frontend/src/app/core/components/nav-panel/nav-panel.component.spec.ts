import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { NavigationStart } from '@angular/router';
import { Subject } from 'rxjs';
import { vi } from 'vitest';

import { NavPanelComponent } from './nav-panel.component';

describe('NavPanelComponent', () => {
  let fixture: ComponentFixture<NavPanelComponent>;
  let component: NavPanelComponent;
  let routerEvents$: Subject<unknown>;

  beforeEach(async () => {
    routerEvents$ = new Subject();

    await TestBed.configureTestingModule({
      imports: [NavPanelComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    // Override router events with our controllable subject
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'events', 'get').mockReturnValue(routerEvents$.asObservable() as any);

    fixture = TestBed.createComponent(NavPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => vi.clearAllMocks());

  it('should create the component', () => {
    // Arrange / Act / Assert
    expect(component).toBeTruthy();
  });

  describe('panel element', () => {
    it('should render a nav element with class nav-panel', () => {
      // Arrange / Act
      const nav = fixture.nativeElement.querySelector('nav.nav-panel') as HTMLElement;

      // Assert
      expect(nav).toBeTruthy();
    });

    it('should have nav-panel--expanded class when isExpanded is true', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', true);

      // Act
      fixture.detectChanges();

      // Assert
      const nav = fixture.nativeElement.querySelector('nav.nav-panel') as HTMLElement;
      expect(nav.classList).toContain('nav-panel--expanded');
    });

    it('should not have nav-panel--expanded class when isExpanded is false', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', false);

      // Act
      fixture.detectChanges();

      // Assert
      const nav = fixture.nativeElement.querySelector('nav.nav-panel') as HTMLElement;
      expect(nav.classList).not.toContain('nav-panel--expanded');
    });
  });

  describe('settings button', () => {
    it('should render the settings button', () => {
      // Arrange / Act
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;

      // Assert
      expect(btn).toBeTruthy();
    });

    it('should show the label span when expanded', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', true);

      // Act
      fixture.detectChanges();

      // Assert
      const label = fixture.nativeElement.querySelector('.nav-panel__label') as HTMLElement;
      expect(label).toBeTruthy();
    });

    it('should not show the label span when collapsed', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', false);

      // Act
      fixture.detectChanges();

      // Assert
      const label = fixture.nativeElement.querySelector('.nav-panel__label') as HTMLElement;
      expect(label).toBeNull();
    });

    it('should display "Settings" as the label text', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', true);

      // Act
      fixture.detectChanges();

      // Assert
      const label = fixture.nativeElement.querySelector('.nav-panel__label') as HTMLElement;
      expect(label.textContent?.trim()).toBe('Settings');
    });

    it('should set aria-expanded to false by default', () => {
      // Arrange / Act
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;

      // Assert
      expect(btn.getAttribute('aria-expanded')).toBe('false');
    });

    it('should set aria-expanded to true when settings is open', () => {
      // Arrange
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;

      // Act
      btn.click();
      fixture.detectChanges();

      // Assert
      const updatedBtn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;
      expect(updatedBtn.getAttribute('aria-expanded')).toBe('true');
    });

    it('should have nav-panel__btn--active class when settings is open', () => {
      // Arrange
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;

      // Act
      btn.click();
      fixture.detectChanges();

      // Assert
      const updatedBtn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;
      expect(updatedBtn.classList).toContain('nav-panel__btn--active');
    });

    it('should not have nav-panel__btn--active class when settings is closed', () => {
      // Arrange / Act
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;

      // Assert
      expect(btn.classList).not.toContain('nav-panel__btn--active');
    });
  });

  describe('handleToggleSettings', () => {
    it('should open the submenu when settings button is clicked', () => {
      // Arrange
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;

      // Act
      btn.click();
      fixture.detectChanges();

      // Assert
      const submenu = fixture.nativeElement.querySelector('.nav-panel__submenu') as HTMLElement;
      expect(submenu).toBeTruthy();
    });

    it('should close the submenu when settings button is clicked again', () => {
      // Arrange
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;
      btn.click();
      fixture.detectChanges();

      // Act
      btn.click();
      fixture.detectChanges();

      // Assert
      const submenu = fixture.nativeElement.querySelector('.nav-panel__submenu') as HTMLElement;
      expect(submenu).toBeNull();
    });

    it('should stop event propagation when settings button is clicked', () => {
      // Arrange
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;
      const event = new MouseEvent('click', { bubbles: true });
      const stopPropagationSpy = vi.spyOn(event, 'stopPropagation');

      // Act
      btn.dispatchEvent(event);
      fixture.detectChanges();

      // Assert
      expect(stopPropagationSpy).toHaveBeenCalledWith();
    });
  });

  describe('submenu', () => {
    beforeEach(() => {
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;
      btn.click();
      fixture.detectChanges();
    });

    it('should not render the submenu by default', () => {
      // Arrange — close it again
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;
      btn.click();
      fixture.detectChanges();

      // Act / Assert
      const submenu = fixture.nativeElement.querySelector('.nav-panel__submenu') as HTMLElement;
      expect(submenu).toBeNull();
    });

    it('should render the submenu when settings is open', () => {
      // Arrange / Act (opened in beforeEach)
      const submenu = fixture.nativeElement.querySelector('.nav-panel__submenu') as HTMLElement;

      // Assert
      expect(submenu).toBeTruthy();
    });

    it('should set left to sidebar-width-expanded when panel is expanded', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', true);
      fixture.detectChanges();

      // Act
      const submenu = fixture.nativeElement.querySelector('.nav-panel__submenu') as HTMLElement;

      // Assert
      expect(submenu.style.left).toBe('var(--sidebar-width-expanded)');
    });

    it('should set left to sidebar-width-collapsed when panel is collapsed', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', false);
      fixture.detectChanges();

      // Act
      const submenu = fixture.nativeElement.querySelector('.nav-panel__submenu') as HTMLElement;

      // Assert
      expect(submenu.style.left).toBe('var(--sidebar-width-collapsed)');
    });

    it('should set top to the button getBoundingClientRect top when opened', () => {
      // Arrange — submenu already opened in beforeEach via btn.click()
      // The button's getBoundingClientRect().top in jsdom is 0 (no layout engine)
      const submenu = fixture.nativeElement.querySelector('.nav-panel__submenu') as HTMLElement;

      // Assert
      expect(submenu.style.top).toBe('0px');
    });

    it('should not update top when submenu is closed by clicking settings again', () => {
      // Arrange — submenu open from beforeEach
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;

      // Act — close
      btn.click();
      fixture.detectChanges();

      // Re-open to confirm top is still captured from the button rect
      btn.click();
      fixture.detectChanges();

      // Assert — top is still set (jsdom returns 0)
      const submenu = fixture.nativeElement.querySelector('.nav-panel__submenu') as HTMLElement;
      expect(submenu.style.top).toBe('0px');
    });

    it('should render the Version link in the submenu', () => {
      // Arrange / Act
      const link = fixture.nativeElement.querySelector('.nav-panel__submenu-item') as HTMLAnchorElement;

      // Assert
      expect(link).toBeTruthy();
    });

    it('should display "Version" as the submenu item text', () => {
      // Arrange / Act
      const link = fixture.nativeElement.querySelector('.nav-panel__submenu-item') as HTMLAnchorElement;

      // Assert
      expect(link.textContent?.trim()).toBe('Version');
    });
  });

  describe('handleToggleCollapse', () => {
    it('should collapse the panel when toggle button is clicked while expanded', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', true);
      fixture.detectChanges();
      const toggleBtn = fixture.nativeElement.querySelector('.nav-panel__toggle') as HTMLButtonElement;

      // Act
      toggleBtn.click();
      fixture.detectChanges();

      // Assert
      const nav = fixture.nativeElement.querySelector('nav.nav-panel') as HTMLElement;
      expect(nav.classList).not.toContain('nav-panel--expanded');
    });

    it('should expand the panel when toggle button is clicked while collapsed', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', false);
      fixture.detectChanges();
      const toggleBtn = fixture.nativeElement.querySelector('.nav-panel__toggle') as HTMLButtonElement;

      // Act
      toggleBtn.click();
      fixture.detectChanges();

      // Assert
      const nav = fixture.nativeElement.querySelector('nav.nav-panel') as HTMLElement;
      expect(nav.classList).toContain('nav-panel--expanded');
    });

    it('should emit the updated isExpanded value when toggled', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', true);
      fixture.detectChanges();
      const emittedValues: boolean[] = [];
      component.isExpanded.subscribe((v) => emittedValues.push(v));
      const toggleBtn = fixture.nativeElement.querySelector('.nav-panel__toggle') as HTMLButtonElement;

      // Act
      toggleBtn.click();

      // Assert
      expect(emittedValues).toContain(false);
    });
  });

  describe('toggle button', () => {
    it('should show aria-label "Collapse navigation" when expanded', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', true);

      // Act
      fixture.detectChanges();

      // Assert
      const btn = fixture.nativeElement.querySelector('.nav-panel__toggle') as HTMLButtonElement;
      expect(btn.getAttribute('aria-label')).toBe('Collapse navigation');
    });

    it('should show aria-label "Expand navigation" when collapsed', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', false);

      // Act
      fixture.detectChanges();

      // Assert
      const btn = fixture.nativeElement.querySelector('.nav-panel__toggle') as HTMLButtonElement;
      expect(btn.getAttribute('aria-label')).toBe('Expand navigation');
    });

    it('should render chevron-left icon when expanded', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', true);

      // Act
      fixture.detectChanges();

      // Assert — heroChevronLeft SVG path is distinctive
      const svg = fixture.nativeElement.querySelector('.nav-panel__toggle svg') as SVGElement;
      expect(svg.innerHTML).toContain('M15.75 19.5 8.25 12l7.5-7.5');
    });

    it('should render chevron-right icon when collapsed', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', false);

      // Act
      fixture.detectChanges();

      // Assert — heroChevronRight SVG path is distinctive
      const svg = fixture.nativeElement.querySelector('.nav-panel__toggle svg') as SVGElement;
      expect(svg.innerHTML).toContain('m8.25 4.5 7.5 7.5-7.5 7.5');
    });
  });

  describe('click outside behaviour', () => {
    it('should close the submenu when clicking outside the component', () => {
      // Arrange
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;
      btn.click();
      fixture.detectChanges();

      // Act
      document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }));
      fixture.detectChanges();

      // Assert
      const submenu = fixture.nativeElement.querySelector('.nav-panel__submenu') as HTMLElement;
      expect(submenu).toBeNull();
    });

    it('should not close the submenu when clicking inside the component', () => {
      // Arrange
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;
      btn.click();
      fixture.detectChanges();

      // Act
      fixture.nativeElement.dispatchEvent(new MouseEvent('click', { bubbles: true }));
      fixture.detectChanges();

      // Assert
      const submenu = fixture.nativeElement.querySelector('.nav-panel__submenu') as HTMLElement;
      expect(submenu).toBeTruthy();
    });

    it('should not close the submenu when it is already closed and a document click occurs', () => {
      // Arrange — submenu is closed by default

      // Act
      document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }));
      fixture.detectChanges();

      // Assert — no errors and submenu remains absent
      const submenu = fixture.nativeElement.querySelector('.nav-panel__submenu') as HTMLElement;
      expect(submenu).toBeNull();
    });
  });

  describe('navigation behaviour', () => {
    it('should close the submenu on NavigationStart', () => {
      // Arrange
      const btn = fixture.nativeElement.querySelector('.nav-panel__btn') as HTMLButtonElement;
      btn.click();
      fixture.detectChanges();

      // Act
      routerEvents$.next(new NavigationStart(1, '/version'));
      fixture.detectChanges();

      // Assert
      const submenu = fixture.nativeElement.querySelector('.nav-panel__submenu') as HTMLElement;
      expect(submenu).toBeNull();
    });

    it('should not affect panel expanded state on NavigationStart', () => {
      // Arrange
      fixture.componentRef.setInput('isExpanded', true);
      fixture.detectChanges();

      // Act
      routerEvents$.next(new NavigationStart(1, '/version'));
      fixture.detectChanges();

      // Assert
      const nav = fixture.nativeElement.querySelector('nav.nav-panel') as HTMLElement;
      expect(nav.classList).toContain('nav-panel--expanded');
    });
  });
});
