'use client';

import {
  ColumnFiltersState,
  SortingState,
  VisibilityState,
  flexRender,
  getCoreRowModel,
  getExpandedRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  useReactTable,
} from '@tanstack/react-table';
import ReactPaginate from 'react-paginate';

import { Input } from '@/components/ui/input';

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import TablePagination from '@/app/[locale]/(protected)/table/react-table/example2/table-pagination';
import { cn } from '@/lib/utils';
import Image from 'next/image';
import { Fragment, useEffect, useState } from 'react';
import { set } from 'lodash';

export type TableProps = {
  data: any[];
  columns: any[];
  isLoading?: boolean;
  title: string;
  header?: React.ReactNode;
  overflow?: boolean;
  pageSize?: number;
  pageIndex?: number;
  totalItems?: number;
  totalPages?: number;
  currentPage?: number;
  setCurrentPage?: (currentPage: number) => void;
  hasNextPage?: boolean;
  hasPreviousPage?: boolean;
  setPageSize?: (pageSize: number) => void;
  setPageIndex?: (pageIndex: number) => void;
};

const TableCustom = ({
  currentPage,
  hasNextPage,
  hasPreviousPage,
  pageIndex,
  pageSize,
  setPageIndex,
  setPageSize,
  setCurrentPage,
  totalItems,
  totalPages,
  data,
  columns,
  isLoading,
  title,
  header,
  overflow,
}: TableProps) => {
  const [sorting, setSorting] = useState<SortingState>([]);

  const [expanded, setExpanded] = useState({});
  const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({});
  const [rowSelection, setRowSelection] = useState({});

  useEffect(() => {
    console.log('currentPage', currentPage);
    console.log('pageIndex', pageIndex);
    console.log('pageSize', pageSize);
    console.log('totalItems', totalItems);
    console.log('totalPages', totalPages);
    console.log('hasNextPage', hasNextPage);
    console.log('hasPreviousPage', hasPreviousPage);
  }, [
    currentPage,
    pageIndex,
    pageSize,
    totalItems,
    totalPages,
    hasNextPage,
    hasPreviousPage,
  ]);

  const table = useReactTable({
    data,
    columns,
    state: {
      sorting,
      columnVisibility,
      expanded,
      rowSelection,
    },
    onSortingChange: setSorting,
    onExpandedChange: setExpanded,
    getRowCanExpand: () => true,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    onColumnVisibilityChange: setColumnVisibility,
    onRowSelectionChange: setRowSelection,
    getExpandedRowModel: getExpandedRowModel(),
  });

  return (
    <div className='w-full'>
      <div className='flex items-center py-4 '>
        <div className='flex-1 text-xl font-medium text-default-900'>
          {title}
        </div>
        <div className=''>
          {/* <Input
            placeholder='Filter Status...'
            value={
              (table.getColumn('status')?.getFilterValue() as string) ?? ''
            }
            onChange={(event) =>
              table.getColumn('status')?.setFilterValue(event.target.value)
            }
            className='max-w-sm'
          /> */}
          {header}
        </div>
      </div>

      <Table>
        <TableHeader className='bg-default-200'>
          {table.getHeaderGroups().map((headerGroup) => (
            <TableRow key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <TableHead key={header.id}>
                  {header.isPlaceholder
                    ? null
                    : flexRender(
                        header.column.columnDef.header,
                        header.getContext()
                      )}
                </TableHead>
              ))}
            </TableRow>
          ))}
        </TableHeader>
        <TableBody className={cn(overflow && 'overflow-scroll')}>
          {!isLoading &&
          table.getRowModel() &&
          table.getRowModel().rows?.length ? (
            table.getRowModel().rows.map((row) => (
              <Fragment key={row.id}>
                <TableRow
                  data-state={row.getIsSelected() ? 'selected' : undefined}
                  className='cursor-pointer'
                >
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext()
                      )}
                    </TableCell>
                  ))}
                </TableRow>
                {row.getIsExpanded() && (
                  <TableRow>
                    <TableCell colSpan={columns.length} className=' p-4'>
                      {row.original.expandedContent}
                    </TableCell>
                  </TableRow>
                )}
              </Fragment>
            ))
          ) : (
            <TableRow>
              <TableCell colSpan={columns.length} className='h-24 text-center'>
                No results.
              </TableCell>
            </TableRow>
          )}
          {isLoading && (
            <TableRow>
              <TableCell colSpan={columns.length} className='h-32 text-center'>
                <Image src='/LoadingAnimation.webm' alt='loading'></Image>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
      {/* Pagination could be added here if you uncomment and configure TablePagination */}
      {currentPage &&
        pageIndex &&
        pageSize &&
        totalItems &&
        totalPages &&
        hasNextPage !== undefined &&
        hasPreviousPage !== undefined &&
        setPageSize &&
        setPageIndex && (
          <TablePagination
            currentPage={currentPage}
            hasNextPage={hasNextPage}
            hasPreviousPage={hasPreviousPage}
            pageIndex={pageIndex}
            pageSize={pageSize}
            setPageIndex={setPageIndex}
            setPageSize={setPageSize}
            totalItems={totalItems}
            totalPages={totalPages}
          ></TablePagination>
        )}
    </div>
  );
};

export default TableCustom;
