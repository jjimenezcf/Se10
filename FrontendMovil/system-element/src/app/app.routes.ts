import { Route, Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { sectionResolver } from './core/resolvers/section.resolver';
import { sectionLoaderResolver } from './core/resolvers/section-loader.resolver';

export const fileRoute: Route = {
  path: 'ficheros/:domainName/:itemId',
  loadComponent: () =>
    import('./pages/file/file.component').then((c) => c.FileComponent),
  data: { breadcrumb: 'Ficheros' },
};

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'principal',
    pathMatch: 'full',
  },
  {
    path: 'entrar',
    loadComponent: () =>
      import('./pages/login/login.component').then((c) => c.LoginComponent),
    data: { title: 'Entrar' },
  },
  {
    path: '',
    canActivate: [authGuard],
    resolve: { sections: sectionLoaderResolver },
    loadComponent: () =>
      import('./core/components/layout/layout.component').then(
        (c) => c.LayoutComponent
      ),
    children: [
      {
        path: 'principal',
        loadComponent: () =>
          import('./pages/main/main.component').then((c) => c.MainComponent),
        data: {
          breadcrumb: 'Principal',
        },
      },
      {
        path: ':sectionPath',
        data: {
          breadcrumb: { alias: 'sectionName' },
        },
        resolve: { section: sectionResolver },
        children: [
          {
            path: '',
            loadComponent: () =>
              import('./pages/section/section.component').then(
                (c) => c.SectionComponent
              ),
          },
          fileRoute,
          {
            path: 'archivadores/:sourceCabinetId',
            data: {
              breadcrumb: 'Archivadores',
            },
            children: [
              {
                path: '',
                loadComponent: () =>
                  import('./pages/cabinet/cabinet.component').then(
                    (c) => c.CabinetComponent
                  ),
              },
              fileRoute,
              {
                path: 'carpetas/:cabinetId',
                data: {
                  breadcrumb: 'Carpetas',
                },
                children: [
                  {
                    path: '',
                    loadComponent: () =>
                      import('./pages/folder/folder.component').then(
                        (c) => c.FolderComponent
                      ),
                  },
                  fileRoute,
                ],
              },
            ],
          },
        ],
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'main',
    pathMatch: 'full',
  },
];
