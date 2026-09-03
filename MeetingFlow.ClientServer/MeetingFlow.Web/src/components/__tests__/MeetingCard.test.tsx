import { MemoryRouter } from 'react-router-dom';
import { render, screen } from '@testing-library/react';
import MeetingCard from '../MeetingCard';
import { expect, test } from 'vitest';

const meeting_demo_pub = {
  id: "1",
  title: "Demo Meeting",
  description: "This is a demo meeting for testing purposes.",
  status: "Published",
  startsAt: "2023-10-15T10:00:00Z",
  endsAt: "2023-10-15T11:00:00Z",
  createdAt: "2023-10-10T08:00:00Z",
  updatedAt: "2023-10-10T08:00:00Z",    
  internalNotes: null,
  adminOnlyCode: null,
  venueId: "1",
  venue: undefined,
  sessions: [],
  registrations: [],
  feedback: [],
}

const meeting_demo_draft = {
  id: "2",
  title: "Demo Meeting",
  description: "This is a demo meeting for testing purposes.",
  status: "Draft",
  startsAt: "2023-10-15T10:00:00Z",
  endsAt: "2023-10-15T11:00:00Z",
  createdAt: "2023-10-10T08:00:00Z",
  updatedAt: "2023-10-10T08:00:00Z",    
  internalNotes: null,
  adminOnlyCode: null,
  venueId: "2",
  venue: undefined,
  sessions: [],
  registrations: [],
  feedback: [],
}

const meeting_demo_cancelled = {
  id: "3",
  title: "Demo Meeting",
  description: "This is a demo meeting for testing purposes.",
  status: "Cancelled",
  startsAt: "2023-10-15T10:00:00Z",
  endsAt: "2023-10-15T11:00:00Z",
  createdAt: "2023-10-10T08:00:00Z",
  updatedAt: "2023-10-10T08:00:00Z",    
  internalNotes: null,
  adminOnlyCode: null,
  venueId: "3",
  venue: undefined,
  sessions: [],
  registrations: [],
  feedback: [],
}

test("renders Published badge", () => {
  render(
    <MemoryRouter>
      <MeetingCard meeting={meeting_demo_pub} />
    </MemoryRouter>
  );

  expect(screen.getByText("Published")).toBeInTheDocument();
});


test("renders Draft badge", () => {
  render(
    <MemoryRouter>
      <MeetingCard meeting={meeting_demo_draft} />
    </MemoryRouter>
  );

  expect(screen.getByText("Draft")).toBeInTheDocument();
});

test("renders Cancelled badge", () => {
  render(
    <MemoryRouter>
      <MeetingCard meeting={meeting_demo_cancelled} />
    </MemoryRouter>
  );

  expect(screen.getByText("Cancelled")).toBeInTheDocument();
});