/**
 * 看板卡片狀態 Enum
 */
export enum CardStatus {
  /** 規劃中 */
  Plan = 0,
  /** 待辦事項 */
  ToDo = 1,
  /** 進行中 */
  Doing = 2,
  /** 已完成 */
  Done = 3,
  /** 自動結案 (上個月已完成卡片自動結案，預設不顯示於看板) */
  AutoClosed = 4
}

/**
 * 看板卡片範疇 Enum
 */
export enum CardScope {
  /** 個人卡片 (僅卡片擁有者可見) */
  Personal = 0,
  /** 組織/部門卡片 (同部門成員與被指派人可見) */
  Organization = 1
}

/**
 * 卡片內部細項任務模型 (CardTask Interface)
 */
export interface CardTask {
  id: string;
  cardId: string;
  title: string;
  isCompleted: boolean;
  assigneeId: number | null;
  assigneeName: string | null;
  sequenceOrder: number;
  dueDate: string | null;
  devOpsUrl: string | null;
  createdAt: string;
  updatedAt: string;
}

/**
 * 看板卡片完整模型 (KanbanCard Interface)
 */
export interface KanbanCard {
  id: string;
  title: string;
  description: string | null;
  status: CardStatus;
  scope: CardScope;
  ownerId: number;
  ownerName: string | null;
  departmentId: number | null;
  departmentName: string | null;
  dueDate: string | null;
  sequenceOrder: number;
  devOpsUrl: string | null;
  deletedAt: string | null;
  createdAt: string;
  updatedAt: string;
  tasks: CardTask[];
}

/**
 * 使用者摘要模型 (提供指派成員選單使用)
 */
export interface UserSummary {
  id: number;
  name: string;
}

/**
 * 移動卡片與調整排序之 Request Payload
 */
export interface MoveCardRequest {
  status: CardStatus;
  sequenceOrder: number;
  updatedAt: string;
}

/**
 * 建立卡片之 Request Payload
 */
export interface CreateCardRequest {
  title: string;
  description: string | null;
  scope: CardScope;
  departmentId: number | null;
  dueDate: string | null;
  sequenceOrder: number;
  devOpsUrl: string | null;
}

/**
 * 更新卡片之 Request Payload
 */
export interface UpdateCardRequest {
  title: string | null;
  description: string | null;
  scope: CardScope | null;
  departmentId: number | null;
  dueDate: string | null;
  devOpsUrl: string | null;
  updatedAt: string | null;
}

/**
 * 建立 Task 之 Request Payload
 */
export interface CreateTaskRequest {
  title: string;
  assigneeId: number | null;
  sequenceOrder: number;
  dueDate: string | null;
  devOpsUrl: string | null;
}

/**
 * 變更 Task 指派人員之 Request Payload
 */
export interface AssignTaskRequest {
  assigneeId: number | null;
  updatedAt: string | null;
}
