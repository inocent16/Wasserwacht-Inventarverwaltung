begin;

select plan(1);

insert into auth.users (id, email)
values ('44444444-4444-4444-8444-444444444444', 'inventory-write-admin@example.test');

update public.profiles
set role = 'administrator'
where id = '44444444-4444-4444-8444-444444444444';

set local role authenticated;
set local request.jwt.claim.sub = '44444444-4444-4444-8444-444444444444';
set local request.jwt.claim.role = 'authenticated';

select lives_ok(
  $test$
  do $body$
  declare
    v_article jsonb;
    v_location_a jsonb;
    v_location_b jsonb;
    v_first_receipt jsonb;
    v_second_receipt jsonb;
    v_other_batch_receipt jsonb;
    v_transfer jsonb;
    v_removal jsonb;
    v_article_id uuid;
    v_location_a_id uuid;
    v_location_b_id uuid;
    v_source_stock_id uuid;
    v_other_batch_stock_id uuid;
    v_target_stock_id uuid;
    v_audit_before integer;
    v_audit_after integer;
    v_stock_count integer;
  begin
    select count(*)::integer into v_audit_before
    from public.audit_log;

    v_article := public.create_article(
      'pgTAP rescue blanket',
      'Equipment',
      'Function integration test',
      null
    );
    v_article_id := (v_article ->> 'id')::uuid;

    v_location_a := public.create_location('pgTAP vehicle', null);
    v_location_a_id := (v_location_a ->> 'id')::uuid;

    v_location_b := public.create_location('pgTAP station', null);
    v_location_b_id := (v_location_b ->> 'id')::uuid;

    v_first_receipt := public.receive_stock(
      v_article_id, v_location_a_id, 5, null, null, null, 'first delivery'
    );
    v_source_stock_id := (v_first_receipt -> 'stock_item' ->> 'id')::uuid;

    v_second_receipt := public.receive_stock(
      v_article_id, v_location_a_id, 2, null, null, null, 'same lot'
    );

    if (v_second_receipt -> 'stock_item' ->> 'id')::uuid <> v_source_stock_id then
      raise exception 'identical optional attributes should merge into the same stock item';
    end if;

    if (v_second_receipt -> 'stock_item' ->> 'quantity')::integer <> 7 then
      raise exception 'merged quantity should be 7';
    end if;

    v_other_batch_receipt := public.receive_stock(
      v_article_id, v_location_a_id, 1, null, 'BATCH-B', 'Manufacturer B', null
    );
    v_other_batch_stock_id := (v_other_batch_receipt -> 'stock_item' ->> 'id')::uuid;

    if v_other_batch_stock_id = v_source_stock_id then
      raise exception 'different known batch/manufacturer must remain a separate stock item';
    end if;

    select count(*)::integer into v_stock_count
    from public.stock_items
    where article_id = v_article_id and location_id = v_location_a_id;

    if v_stock_count <> 2 then
      raise exception 'expected exactly two distinct stock identities at source location';
    end if;

    v_transfer := public.transfer_stock(
      v_source_stock_id, v_location_b_id, 3, 'test transfer'
    );
    v_target_stock_id := (v_transfer -> 'target_stock_item' ->> 'id')::uuid;

    if (v_transfer -> 'source_stock_item' ->> 'quantity')::integer <> 4 then
      raise exception 'source quantity after transfer should be 4';
    end if;

    if (v_transfer -> 'target_stock_item' ->> 'quantity')::integer <> 3 then
      raise exception 'target quantity after transfer should be 3';
    end if;

    v_removal := public.remove_stock_batch(
      v_location_b_id,
      jsonb_build_array(
        jsonb_build_object('stockItemId', v_target_stock_id, 'quantity', 3)
      ),
      'test full removal'
    );

    if exists (select 1 from public.stock_items where id = v_target_stock_id) then
      raise exception 'fully removed target stock item must be deleted';
    end if;

    if not exists (
      select 1 from public.stock_items
      where id = v_source_stock_id and quantity = 4
    ) then
      raise exception 'source stock item should remain with quantity 4';
    end if;

    select count(*)::integer into v_audit_after
    from public.audit_log;

    if v_audit_after - v_audit_before <> 8 then
      raise exception 'expected one audit row for each of 8 successful RPC actions; got %',
        v_audit_after - v_audit_before;
    end if;
  end
  $body$;
  $test$,
  'receipt merge, distinct lots, transfer, batch removal and one-audit-row-per-action work atomically'
);

select * from finish();
rollback;
