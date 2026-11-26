begin;

select plan(16);

select has_table('public', 'profiles', 'profiles table exists');
select has_table('public', 'articles', 'articles table exists');
select has_table('public', 'locations', 'locations table exists');
select has_table('public', 'stock_items', 'stock_items table exists');
select has_table('public', 'audit_log', 'the one central audit_log table exists');

select ok(
  (select relrowsecurity from pg_class where oid = 'public.articles'::regclass),
  'articles has row level security enabled'
);
select ok(
  (select relrowsecurity from pg_class where oid = 'public.locations'::regclass),
  'locations has row level security enabled'
);
select ok(
  (select relrowsecurity from pg_class where oid = 'public.stock_items'::regclass),
  'stock_items has row level security enabled'
);
select ok(
  (select relrowsecurity from pg_class where oid = 'public.audit_log'::regclass),
  'audit_log has row level security enabled'
);

select ok(
  exists (
    select 1
    from pg_index i
    join pg_class c on c.oid = i.indexrelid
    where i.indrelid = 'public.stock_items'::regclass
      and c.relname = 'stock_items_identity_ux'
      and i.indisunique
      and i.indnullsnotdistinct
  ),
  'stock identity is unique including NULL optional attributes'
);

select ok(
  to_regclass('public.stock_movements') is null,
  'there is no separate stock_movements table'
);

select ok(
  to_regprocedure('public.receive_stock(uuid,uuid,integer,date,text,text,text)') is not null,
  'receive_stock RPC exists'
);
select ok(
  to_regprocedure('public.transfer_stock(uuid,uuid,integer,text)') is not null,
  'transfer_stock RPC exists'
);
select ok(
  to_regprocedure('public.remove_stock_batch(uuid,jsonb,text)') is not null,
  'remove_stock_batch RPC exists'
);

select ok(
  not has_function_privilege(
    'anon',
    'public.receive_stock(uuid,uuid,integer,date,text,text,text)',
    'EXECUTE'
  ),
  'anon cannot execute inventory write RPCs'
);
select ok(
  has_function_privilege(
    'authenticated',
    'public.receive_stock(uuid,uuid,integer,date,text,text,text)',
    'EXECUTE'
  ),
  'authenticated can call inventory RPCs (the function checks administrator role)'
);

select * from finish();
rollback;
