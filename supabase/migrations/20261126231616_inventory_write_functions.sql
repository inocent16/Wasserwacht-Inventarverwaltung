-- Audited write API for the inventory domain.
-- SECURITY DEFINER functions are the only write path for authenticated users.
-- Every successful business call writes exactly one row to public.audit_log.

create or replace function public.require_inventory_admin()
returns uuid
language plpgsql
stable
security definer
set search_path = ''
as $$
declare
  v_user_id uuid;
begin
  v_user_id := auth.uid();

  if v_user_id is null or not public.is_admin() then
    raise exception 'Administrator access required'
      using errcode = '42501';
  end if;

  return v_user_id;
end;
$$;

revoke all on function public.require_inventory_admin() from public, anon, authenticated;

create or replace function public.write_audit_log(
  p_event_type text,
  p_before_state jsonb,
  p_after_state jsonb,
  p_metadata jsonb default '{}'::jsonb
)
returns uuid
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_actor_id uuid;
  v_actor_name text;
  v_audit_id uuid;
begin
  v_actor_id := public.require_inventory_admin();

  select coalesce(nullif(u.email, ''), nullif(p.email, ''), v_actor_id::text)
    into v_actor_name
  from auth.users u
  left join public.profiles p on p.id = u.id
  where u.id = v_actor_id;

  if v_actor_name is null then
    v_actor_name := v_actor_id::text;
  end if;

  insert into public.audit_log (
    actor_user_id,
    actor_name_snapshot,
    event_type,
    before_state,
    after_state,
    metadata
  )
  values (
    v_actor_id,
    left(v_actor_name, 200),
    left(p_event_type, 100),
    p_before_state,
    p_after_state,
    coalesce(p_metadata, '{}'::jsonb)
  )
  returning id into v_audit_id;

  return v_audit_id;
end;
$$;

revoke all on function public.write_audit_log(text, jsonb, jsonb, jsonb)
  from public, anon, authenticated;

-- ---------------------------------------------------------------------------
-- Article CRUD. Direct table DML is revoked; all changes create one audit row.
-- ---------------------------------------------------------------------------

create or replace function public.create_article(
  p_name text,
  p_category text,
  p_description text default null,
  p_manufacturer text default null
)
returns jsonb
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_article public.articles%rowtype;
begin
  perform public.require_inventory_admin();

  if p_name is null or length(btrim(p_name)) = 0
     or p_category is null or length(btrim(p_category)) = 0 then
    raise exception 'Article name and category are required'
      using errcode = '22023';
  end if;

  insert into public.articles (name, category, description, manufacturer)
  values (
    btrim(p_name),
    btrim(p_category),
    nullif(btrim(p_description), ''),
    nullif(btrim(p_manufacturer), '')
  )
  returning * into v_article;

  perform public.write_audit_log(
    'article_created',
    null,
    to_jsonb(v_article),
    '{}'::jsonb
  );

  return to_jsonb(v_article);
end;
$$;

create or replace function public.update_article(
  p_article_id uuid,
  p_name text,
  p_category text,
  p_description text default null,
  p_manufacturer text default null
)
returns jsonb
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_before public.articles%rowtype;
  v_after public.articles%rowtype;
begin
  perform public.require_inventory_admin();

  if p_name is null or length(btrim(p_name)) = 0
     or p_category is null or length(btrim(p_category)) = 0 then
    raise exception 'Article name and category are required'
      using errcode = '22023';
  end if;

  select * into v_before
  from public.articles
  where id = p_article_id
  for update;

  if not found then
    raise exception 'Article not found' using errcode = 'P0002';
  end if;

  update public.articles
  set name = btrim(p_name),
      category = btrim(p_category),
      description = nullif(btrim(p_description), ''),
      manufacturer = nullif(btrim(p_manufacturer), ''),
      updated_at = now()
  where id = p_article_id
  returning * into v_after;

  perform public.write_audit_log(
    'article_updated',
    to_jsonb(v_before),
    to_jsonb(v_after),
    '{}'::jsonb
  );

  return to_jsonb(v_after);
end;
$$;

create or replace function public.delete_article(p_article_id uuid)
returns jsonb
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_before public.articles%rowtype;
begin
  perform public.require_inventory_admin();

  select * into v_before
  from public.articles
  where id = p_article_id
  for update;

  if not found then
    raise exception 'Article not found' using errcode = 'P0002';
  end if;

  if exists (select 1 from public.stock_items s where s.article_id = p_article_id) then
    raise exception 'Article still has stock items and cannot be deleted'
      using errcode = '23503';
  end if;

  delete from public.articles where id = p_article_id;

  perform public.write_audit_log(
    'article_deleted',
    to_jsonb(v_before),
    null,
    '{}'::jsonb
  );

  return jsonb_build_object('deleted', true, 'id', p_article_id);
end;
$$;

-- ---------------------------------------------------------------------------
-- Location CRUD.
-- ---------------------------------------------------------------------------

create or replace function public.create_location(
  p_name text,
  p_description text default null
)
returns jsonb
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_location public.locations%rowtype;
begin
  perform public.require_inventory_admin();

  if p_name is null or length(btrim(p_name)) = 0 then
    raise exception 'Location name is required' using errcode = '22023';
  end if;

  insert into public.locations (name, description)
  values (btrim(p_name), nullif(btrim(p_description), ''))
  returning * into v_location;

  perform public.write_audit_log(
    'location_created',
    null,
    to_jsonb(v_location),
    '{}'::jsonb
  );

  return to_jsonb(v_location);
end;
$$;

create or replace function public.update_location(
  p_location_id uuid,
  p_name text,
  p_description text default null
)
returns jsonb
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_before public.locations%rowtype;
  v_after public.locations%rowtype;
begin
  perform public.require_inventory_admin();

  if p_name is null or length(btrim(p_name)) = 0 then
    raise exception 'Location name is required' using errcode = '22023';
  end if;

  select * into v_before
  from public.locations
  where id = p_location_id
  for update;

  if not found then
    raise exception 'Location not found' using errcode = 'P0002';
  end if;

  update public.locations
  set name = btrim(p_name),
      description = nullif(btrim(p_description), ''),
      updated_at = now()
  where id = p_location_id
  returning * into v_after;

  perform public.write_audit_log(
    'location_updated',
    to_jsonb(v_before),
    to_jsonb(v_after),
    '{}'::jsonb
  );

  return to_jsonb(v_after);
end;
$$;

create or replace function public.delete_location(p_location_id uuid)
returns jsonb
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_before public.locations%rowtype;
begin
  perform public.require_inventory_admin();

  select * into v_before
  from public.locations
  where id = p_location_id
  for update;

  if not found then
    raise exception 'Location not found' using errcode = 'P0002';
  end if;

  if exists (select 1 from public.stock_items s where s.location_id = p_location_id) then
    raise exception 'Location still has stock items and cannot be deleted'
      using errcode = '23503';
  end if;

  delete from public.locations where id = p_location_id;

  perform public.write_audit_log(
    'location_deleted',
    to_jsonb(v_before),
    null,
    '{}'::jsonb
  );

  return jsonb_build_object('deleted', true, 'id', p_location_id);
end;
$$;

-- ---------------------------------------------------------------------------
-- Stock identity lock helper.
-- A stable key serializes concurrent upserts of the same stock identity.
-- NULL attributes are part of the identity, matching NULLS NOT DISTINCT.
-- ---------------------------------------------------------------------------

create or replace function public.stock_identity_lock_key(
  p_article_id uuid,
  p_location_id uuid,
  p_expiration_date date,
  p_batch_number text,
  p_manufacturer text
)
returns text
language sql
immutable
set search_path = pg_catalog
as $$
  select jsonb_build_array(
    p_article_id,
    p_location_id,
    p_expiration_date,
    p_batch_number,
    p_manufacturer
  )::text;
$$;

revoke all on function public.stock_identity_lock_key(uuid, uuid, date, text, text)
  from public, anon, authenticated;

-- ---------------------------------------------------------------------------
-- Stock receipt.
-- Exact optional-attribute equality is deliberately conservative: NULL batch
-- does not merge with a known batch; manufacturer must also match exactly.
-- ---------------------------------------------------------------------------

create or replace function public.receive_stock(
  p_article_id uuid,
  p_location_id uuid,
  p_quantity integer,
  p_expiration_date date default null,
  p_batch_number text default null,
  p_manufacturer text default null,
  p_note text default null
)
returns jsonb
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_batch text := nullif(btrim(p_batch_number), '');
  v_manufacturer text := nullif(btrim(p_manufacturer), '');
  v_stock public.stock_items%rowtype;
  v_before jsonb;
  v_created boolean := false;
begin
  perform public.require_inventory_admin();

  if p_quantity is null or p_quantity <= 0 then
    raise exception 'Quantity must be a positive whole number'
      using errcode = '22023';
  end if;

  if not exists (select 1 from public.articles a where a.id = p_article_id) then
    raise exception 'Article not found' using errcode = 'P0002';
  end if;

  if not exists (select 1 from public.locations l where l.id = p_location_id) then
    raise exception 'Location not found' using errcode = 'P0002';
  end if;

  perform pg_advisory_xact_lock(
    hashtextextended(
      public.stock_identity_lock_key(
        p_article_id, p_location_id, p_expiration_date, v_batch, v_manufacturer
      ),
      0
    )
  );

  select * into v_stock
  from public.stock_items s
  where s.article_id = p_article_id
    and s.location_id = p_location_id
    and s.expiration_date is not distinct from p_expiration_date
    and s.batch_number is not distinct from v_batch
    and s.manufacturer is not distinct from v_manufacturer
  for update;

  if found then
    v_before := to_jsonb(v_stock);

    update public.stock_items
    set quantity = quantity + p_quantity,
        updated_at = now()
    where id = v_stock.id
    returning * into v_stock;
  else
    insert into public.stock_items (
      article_id, location_id, quantity, expiration_date, batch_number, manufacturer
    )
    values (
      p_article_id, p_location_id, p_quantity, p_expiration_date, v_batch, v_manufacturer
    )
    returning * into v_stock;

    v_before := null;
    v_created := true;
  end if;

  perform public.write_audit_log(
    'stock_received',
    v_before,
    to_jsonb(v_stock),
    jsonb_strip_nulls(jsonb_build_object('note', nullif(btrim(p_note), '')))
  );

  return jsonb_build_object(
    'stock_item', to_jsonb(v_stock),
    'created', v_created
  );
end;
$$;

-- ---------------------------------------------------------------------------
-- Stock transfer.
-- Both source and destination identity locks are acquired in deterministic
-- order before row locks to prevent transfer-vs-transfer lock inversion.
-- ---------------------------------------------------------------------------

create or replace function public.transfer_stock(
  p_stock_item_id uuid,
  p_target_location_id uuid,
  p_quantity integer,
  p_note text default null
)
returns jsonb
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_source public.stock_items%rowtype;
  v_source_initial public.stock_items%rowtype;
  v_target public.stock_items%rowtype;
  v_source_key text;
  v_target_key text;
  v_target_id uuid;
  v_lock_id uuid;
  v_source_before jsonb;
  v_target_before jsonb;
  v_source_after jsonb;
  v_target_after jsonb;
  v_target_found boolean;
begin
  perform public.require_inventory_admin();

  if p_quantity is null or p_quantity <= 0 then
    raise exception 'Quantity must be a positive whole number'
      using errcode = '22023';
  end if;

  select * into v_source_initial
  from public.stock_items
  where id = p_stock_item_id;

  if not found then
    raise exception 'Source stock item not found' using errcode = 'P0002';
  end if;

  if v_source_initial.location_id = p_target_location_id then
    raise exception 'Source and target locations must be different'
      using errcode = '22023';
  end if;

  if not exists (select 1 from public.locations l where l.id = p_target_location_id) then
    raise exception 'Target location not found' using errcode = 'P0002';
  end if;

  v_source_key := public.stock_identity_lock_key(
    v_source_initial.article_id,
    v_source_initial.location_id,
    v_source_initial.expiration_date,
    v_source_initial.batch_number,
    v_source_initial.manufacturer
  );

  v_target_key := public.stock_identity_lock_key(
    v_source_initial.article_id,
    p_target_location_id,
    v_source_initial.expiration_date,
    v_source_initial.batch_number,
    v_source_initial.manufacturer
  );

  if v_source_key < v_target_key then
    perform pg_advisory_xact_lock(hashtextextended(v_source_key, 0));
    perform pg_advisory_xact_lock(hashtextextended(v_target_key, 0));
  else
    perform pg_advisory_xact_lock(hashtextextended(v_target_key, 0));
    perform pg_advisory_xact_lock(hashtextextended(v_source_key, 0));
  end if;

  -- Discover the target row without locking, then lock source + target in the
  -- same UUID order used by batch removals. The identity advisory locks prevent
  -- concurrent insert/merge of these two identities while IDs are resolved.
  select s.id into v_target_id
  from public.stock_items s
  where s.article_id = v_source_initial.article_id
    and s.location_id = p_target_location_id
    and s.expiration_date is not distinct from v_source_initial.expiration_date
    and s.batch_number is not distinct from v_source_initial.batch_number
    and s.manufacturer is not distinct from v_source_initial.manufacturer;

  for v_lock_id in
    select lock_ids.id
    from (values (p_stock_item_id), (v_target_id)) as lock_ids(id)
    where lock_ids.id is not null
    order by lock_ids.id
  loop
    perform 1 from public.stock_items s where s.id = v_lock_id for update;
  end loop;

  select * into v_source
  from public.stock_items
  where id = p_stock_item_id;

  if not found then
    raise exception 'Source stock item no longer exists' using errcode = 'P0002';
  end if;

  if v_source.location_id = p_target_location_id then
    raise exception 'Source and target locations must be different'
      using errcode = '22023';
  end if;

  if p_quantity > v_source.quantity then
    raise exception 'Insufficient stock'
      using errcode = '22023';
  end if;

  select * into v_target
  from public.stock_items s
  where s.article_id = v_source.article_id
    and s.location_id = p_target_location_id
    and s.expiration_date is not distinct from v_source.expiration_date
    and s.batch_number is not distinct from v_source.batch_number
    and s.manufacturer is not distinct from v_source.manufacturer
  for update;

  v_target_found := found;
  v_source_before := to_jsonb(v_source);

  if v_target_found then
    v_target_before := to_jsonb(v_target);
  else
    v_target_before := null;
  end if;

  if p_quantity = v_source.quantity then
    delete from public.stock_items where id = v_source.id;
    v_source_after := null;
  else
    update public.stock_items
    set quantity = quantity - p_quantity,
        updated_at = now()
    where id = v_source.id
    returning * into v_source;

    v_source_after := to_jsonb(v_source);
  end if;

  if v_target_found then
    update public.stock_items
    set quantity = quantity + p_quantity,
        updated_at = now()
    where id = v_target.id
    returning * into v_target;
  else
    insert into public.stock_items (
      article_id, location_id, quantity, expiration_date, batch_number, manufacturer
    )
    values (
      v_source.article_id,
      p_target_location_id,
      p_quantity,
      v_source.expiration_date,
      v_source.batch_number,
      v_source.manufacturer
    )
    returning * into v_target;
  end if;

  v_target_after := to_jsonb(v_target);

  perform public.write_audit_log(
    'stock_transferred',
    jsonb_build_object(
      'source_stock_item', v_source_before,
      'target_stock_item', v_target_before
    ),
    jsonb_build_object(
      'source_stock_item', v_source_after,
      'target_stock_item', v_target_after
    ),
    jsonb_strip_nulls(jsonb_build_object('note', nullif(btrim(p_note), '')))
  );

  return jsonb_build_object(
    'source_stock_item', v_source_after,
    'target_stock_item', v_target_after
  );
end;
$$;

-- ---------------------------------------------------------------------------
-- Batch removal. The caller must identify each exact stock item; no implicit
-- FEFO distribution across lots occurs. All items are locked in UUID order.
-- p_items shape: [{"stockItemId":"<uuid>","quantity":2}, ...]
-- ---------------------------------------------------------------------------

create or replace function public.remove_stock_batch(
  p_source_location_id uuid,
  p_items jsonb,
  p_note text default null
)
returns jsonb
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_request record;
  v_stock public.stock_items%rowtype;
  v_requested_count integer;
  v_distinct_count integer;
  v_invalid_count integer;
  v_found_count integer;
  v_before_items jsonb;
  v_after_items jsonb := '[]'::jsonb;
begin
  perform public.require_inventory_admin();

  if p_items is null or jsonb_typeof(p_items) is distinct from 'array' then
    raise exception 'Items must be a JSON array' using errcode = '22023';
  end if;

  if jsonb_array_length(p_items) = 0 then
    raise exception 'At least one stock item is required' using errcode = '22023';
  end if;

  if not exists (select 1 from public.locations l where l.id = p_source_location_id) then
    raise exception 'Source location not found' using errcode = 'P0002';
  end if;

  if exists (
    select 1
    from jsonb_array_elements(p_items) e(value)
    where jsonb_typeof(e.value) is distinct from 'object'
       or not (e.value ? 'stockItemId')
       or not (e.value ? 'quantity')
       or jsonb_typeof(e.value -> 'stockItemId') is distinct from 'string'
       or jsonb_typeof(e.value -> 'quantity') is distinct from 'number'
       or (e.value ->> 'quantity') !~ '^[1-9][0-9]*$'
  ) then
    raise exception 'Each item requires a UUID stockItemId and a positive integer quantity'
      using errcode = '22023';
  end if;

  select
    count(*)::integer,
    count(distinct x."stockItemId")::integer,
    count(*) filter (where x."stockItemId" is null or x.quantity is null or x.quantity <= 0)::integer
  into v_requested_count, v_distinct_count, v_invalid_count
  from jsonb_to_recordset(p_items) as x("stockItemId" uuid, quantity integer);

  if v_invalid_count > 0 or v_requested_count <> v_distinct_count then
    raise exception 'Duplicate or invalid stock item entries are not allowed'
      using errcode = '22023';
  end if;

  select count(*)::integer into v_found_count
  from public.stock_items s
  join jsonb_to_recordset(p_items) as x("stockItemId" uuid, quantity integer)
    on x."stockItemId" = s.id
  where s.location_id = p_source_location_id;

  if v_found_count <> v_requested_count then
    raise exception 'One or more stock items do not belong to the source location'
      using errcode = '22023';
  end if;

  -- Deterministic row-lock ordering avoids deadlocks between overlapping batches.
  for v_request in
    select x."stockItemId" as stock_item_id, x.quantity
    from jsonb_to_recordset(p_items) as x("stockItemId" uuid, quantity integer)
    order by x."stockItemId"
  loop
    select * into v_stock
    from public.stock_items
    where id = v_request.stock_item_id
      and location_id = p_source_location_id
    for update;

    if not found then
      raise exception 'Stock item changed while the removal was being prepared'
        using errcode = '40001';
    end if;

    if v_request.quantity > v_stock.quantity then
      raise exception 'Insufficient stock for stock item %', v_stock.id
        using errcode = '22023';
    end if;
  end loop;

  select coalesce(jsonb_agg(to_jsonb(s) order by s.id), '[]'::jsonb)
    into v_before_items
  from public.stock_items s
  join jsonb_to_recordset(p_items) as x("stockItemId" uuid, quantity integer)
    on x."stockItemId" = s.id
  where s.location_id = p_source_location_id;

  for v_request in
    select x."stockItemId" as stock_item_id, x.quantity
    from jsonb_to_recordset(p_items) as x("stockItemId" uuid, quantity integer)
    order by x."stockItemId"
  loop
    select * into v_stock
    from public.stock_items
    where id = v_request.stock_item_id
      and location_id = p_source_location_id
    for update;

    if v_request.quantity = v_stock.quantity then
      delete from public.stock_items where id = v_stock.id;
      v_after_items := v_after_items || jsonb_build_array(
        jsonb_build_object('stock_item_id', v_stock.id, 'stock_item', null)
      );
    else
      update public.stock_items
      set quantity = quantity - v_request.quantity,
          updated_at = now()
      where id = v_stock.id
      returning * into v_stock;

      v_after_items := v_after_items || jsonb_build_array(
        jsonb_build_object('stock_item_id', v_stock.id, 'stock_item', to_jsonb(v_stock))
      );
    end if;
  end loop;

  perform public.write_audit_log(
    'stock_removal_batch',
    jsonb_build_object(
      'source_location_id', p_source_location_id,
      'stock_items', v_before_items
    ),
    jsonb_build_object(
      'source_location_id', p_source_location_id,
      'stock_items', v_after_items
    ),
    jsonb_strip_nulls(jsonb_build_object('note', nullif(btrim(p_note), '')))
  );

  return jsonb_build_object(
    'source_location_id', p_source_location_id,
    'stock_items', v_after_items
  );
end;
$$;

-- Every write function checks the database-backed administrator role. No
-- direct table DML is granted to authenticated/anon users.
revoke all on function public.create_article(text, text, text, text)
  from public, anon, authenticated;
revoke all on function public.update_article(uuid, text, text, text, text)
  from public, anon, authenticated;
revoke all on function public.delete_article(uuid)
  from public, anon, authenticated;
revoke all on function public.create_location(text, text)
  from public, anon, authenticated;
revoke all on function public.update_location(uuid, text, text)
  from public, anon, authenticated;
revoke all on function public.delete_location(uuid)
  from public, anon, authenticated;
revoke all on function public.receive_stock(uuid, uuid, integer, date, text, text, text)
  from public, anon, authenticated;
revoke all on function public.transfer_stock(uuid, uuid, integer, text)
  from public, anon, authenticated;
revoke all on function public.remove_stock_batch(uuid, jsonb, text)
  from public, anon, authenticated;

grant execute on function public.create_article(text, text, text, text) to authenticated;
grant execute on function public.update_article(uuid, text, text, text, text) to authenticated;
grant execute on function public.delete_article(uuid) to authenticated;
grant execute on function public.create_location(text, text) to authenticated;
grant execute on function public.update_location(uuid, text, text) to authenticated;
grant execute on function public.delete_location(uuid) to authenticated;
grant execute on function public.receive_stock(uuid, uuid, integer, date, text, text, text) to authenticated;
grant execute on function public.transfer_stock(uuid, uuid, integer, text) to authenticated;
grant execute on function public.remove_stock_batch(uuid, jsonb, text) to authenticated;
