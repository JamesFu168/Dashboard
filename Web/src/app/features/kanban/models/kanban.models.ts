export enum CardStatus {
  Plan = 0,
  ToDo = 1,
  Doing = 2,
  Done = 3
}

export enum CardScope {
  Personal = 0,
  Organization = 1
}

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

export interface UserSummary {
  id: number;
  name: string;
}

export interface MoveCardRequest {
  status: CardStatus;
  sequenceOrder: number;
  updatedAt: string;
}

export interface CreateCardRequest {
  title: string;
  description: string | null;
  scope: CardScope;
  departmentId: number | null;
  dueDate: string | null;
  sequenceOrder: number;
  devOpsUrl: string | null;
}

export interface UpdateCardRequest {
  title: string | null;
  description: string | null;
  scope: CardScope | null;
  departmentId: number | null;
  dueDate: string | null;
  devOpsUrl: string | null;
  updatedAt: string | null;
}

export interface CreateTaskRequest {
  title: string;
  assigneeId: number | null;
  sequenceOrder: number;
  dueDate: string | null;
  devOpsUrl: string | null;
}

export interface AssignTaskRequest {
  assigneeId: number | null;
  updatedAt: string | null;
}
