begin;

select plan(10);

-- auth.users trigger creates the profiles. New profiles are not privileged.
insert into auth.users (id, email)
values
  ('11111111-1111-4111-8111-111111111111', 'inventory-admin@example.test'),
  ('22222222-2222-4222-8222-222222222222', 'inventory-unprivileged@example.test');

update public.profiles
set role = 'administrator'
where id = '11111111-1111-4111-8111-111111111111';

select is(
  (select role::text from public.profiles where id = '22222222-2222-4222-8222-222222222222'),
  null::text,
  'new Auth profiles have no application role by default'
);

-- A fixture row is inserted as the database owner to test SELECT policies.
insert into public.articles (id, name, category)
values (
  '33333333-3333-4333-8333-333333333333',
  'RLS fixture',
  'Test'
);

set local role anon;

select throws_ok(
  $$$select * from public.articles$$$,
  '42501',
  null,
  'anon cannot read inventory tables'
);

select throws_ok(
  $$$select public.create_location('anon-must-not-create', null)$$$,
  '42501',
  null,
  'anon cannot execute audited write RPCs'
);

reset role;

set local role authenticated;
set local request.jwt.claim.sub = '11111111-1111-4111-8111-111111111111';
set local request.jwt.claim.role = 'authenticated';

select is(
  (select count(*)::integer from public.articles where id = '33333333-3333-4333-8333-333333333333'),
  1,
  'administrator can read inventory rows'
);

select throws_ok(
  $$insert into public.articles (name, category) values ('bypass', 'test')$$,
  '42501',
  null,
  'administrator cannot bypass audited RPC with direct table INSERT'
);

select lives_ok(
  $$select public.create_location('pgTAP admin location', 'created by database test')$$,
  'administrator can create a location through the audited RPC'
);

select is(
  (select count(*)::integer from public.audit_log where event_type = 'location_created'),
  1,
  'one successful location RPC produces exactly one AuditLog row'
);

reset role;
select throws_ok(
  $$update public.audit_log set event_type = 'tampered' where event_type = 'location_created'$$,
  '55000',
  null,
  'audit_log trigger prevents UPDATE even when table privileges exist'
);
set local role authenticated;
set local request.jwt.claim.sub = '22222222-2222-4222-8222-222222222222';

select is(
  (select count(*)::integer from public.articles where id = '33333333-3333-4333-8333-333333333333'),
  0,
  'authenticated user without administrator role cannot read inventory rows'
);

select throws_ok(
  $$$select public.create_location('unprivileged-must-not-create', null)$$$,
  '42501',
  null,
  'authenticated user without administrator role cannot write'
);

select * from finish();
rollback;
