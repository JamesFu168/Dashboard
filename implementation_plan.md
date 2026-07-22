# 小型部門看板系統開發執行計畫 (Implementation Plan)

本計畫為「小型部門看板系統」的逐步開發指南，包含後端 .NET 10 Web API、Azure SQL Database 以及前端 Angular 應用的建置與驗證流程。

---

## 階段一：後端基礎建設與資料庫 Entity 設計 (.NET 10 & EF Core)

- [ ] **1.1 建立 .NET 10 Web API 專案骨架**
  - 初始化 ASP.NET Core Web API 專案。
  - 配置 `appsettings.json` 與 Azure SQL Connection String。
  - 導入 JWT Authentication 與 Swagger/OpenAPI。

- [ ] **1.2 建立 EF Core Entities 與 DbContext**
  - 建立 `User`, `Department`, `Card`, `CardTask` 實體。
  - 設定 Enum：`CardStatus` (`Plan`, `ToDo`, `Doing`, `Done`) 與 `CardScope` (`Personal`, `Organization`)。
  - 建立 Model Builder Fluent API 關係與外鍵約束（`OwnerId`, `AssigneeId` 等）。

- [ ] **1.3 資料庫 Migration 與 Initial Seed Data**
  - 執行 `dotnet ef migrations add InitialCreate`。
  - 建立種子資料 (預設部門、測試使用者、初始範例卡片與 Tasks)。

---

## 階段二：後端核心商業邏輯與 API 開發

- [ ] **2.1 Card API 與權限驗證 Policy**
  - 實作 `GET /api/v1/cards`（根據視角過濾 `Personal` / `Organization` 與 `AssigneeId` 可見性）。
  - 實作 `POST /api/v1/cards`（預設建立者為 Owner）。
  - 實作 `PUT /api/v1/cards/{id}/status`（**權限驗證：僅 Card Owner 可調整列表狀態**）。

- [ ] **2.2 Task API 與細項處理**
  - 實作 `POST /api/v1/cards/{cardId}/tasks`（僅 Card Owner 可新增細項與指派人員）。
  - 實作 `PATCH /api/v1/tasks/{taskId}/toggle`（**權限驗證：Card Owner 或被指派者可勾選 Task 完成狀態**）。

- [ ] **2.3 SignalR 即時溝通 Hub 建立**
  - 建立 `KanbanHub` 處理卡片狀態移動、Task 完成狀態變更的廣播事件。

---

## 階段三：前端 Angular 看板系統開發

- [ ] **3.1 專案建置與 UI 庫整合**
  - 建置 Angular 18+ 專案。
  - 導入 `@angular/cdk/drag-drop` 與設計系統 CSS。
  - 建立身份驗證 `AuthInterceptor` 與狀態 Service (Angular Signals)。

- [ ] **3.2 主看板頁面與 4 個 Column 元件**
  - 建立 `KanbanBoardComponent` 展現 **Plan**, **To Do**, **Doing**, **Done** 四個 Column。
  - 建立頂部選單：切換「個人視角 (Personal)」與「組織視角 (Organization)」。

- [ ] **3.3 權限連動與 Drag-and-Drop 限制**
  - 在卡片元件設定 `[cdkDragDisabled]="!isOwner"`，阻止非 Card Owner 拖移卡片。
  - 在細項 Task 設定 Checkbox 停用狀態 `[disabled]="!canToggleTask(task)"`。

- [ ] **3.4 卡片詳情與 Task 管理 Modal**
  - 提供細項 Task 新增、變更指派人員彈窗介面。

---

## 階段四：全系統整合、測試與部署驗證

- [ ] **4.1 端到端整合測試 (E2E / Functional Test)**
  - 驗證場景 A：卡片擁有者移動 Plan -> Doing (成功)。
  - 驗證場景 B：員工 A 看見指派給自己的組織卡片，試圖拖拉 (觸發鎖定阻擋)。
  - 驗證場景 C：員工 A 勾選指派給自己的 Task 為完成 (成功狀態更新)。
  - 驗證場景 D：SignalR 即時更新廣播給同一部門的其他同仁。

- [ ] **4.2 部署準備**
  - 設定 Azure SQL Database 並執行數據庫 Migration。
  - 設定 CI/CD 或本地部署設定。

---

## 📌 驗證標準與品質防線

1. **安全性驗證**：後端必須進行雙重驗證（前端停用按鈕 + 後端 403 Forbidden 政策防禦）。
2. **UI 體驗驗證**：卡片拖拉順暢無卡頓，跨欄位拖拉視覺回饋清晰。
3. **資料一致性**：Task 勾選狀態變更不應影響 Card 狀態，Card 刪除應連帶 Cascade 刪除 Tasks。
